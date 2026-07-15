using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using PayGate.DTOs;
using PayGate.DTOs.Daraja;
using PayGate.Models;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class PaymentService(
    AppDbContext context, 
    IEncryptionService encryptionService, 
    IDarajaService darajaService,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<Payment> ProcessPaymentAsync(CreatePaymentDto dto, Guid clientAppId)
    {
        // 1. Strict Idempotency Check
        if (string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            throw new ArgumentException("IdempotencyKey is required to prevent duplicate charges.");

        var existingPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == dto.IdempotencyKey);
            
        if (existingPayment != null)
        {
            logger.LogWarning("⚠️ Duplicate payment request detected for key: {Key}", dto.IdempotencyKey);
            return existingPayment;
        }

        // 2. Get the Client App
        var clientApp = await context.ClientApps.FindAsync(clientAppId)
            ?? throw new Exception($"Client App {clientAppId} not found.");

        if (!clientApp.IsActive)
            throw new Exception("This Client App is deactivated.");

        // 3. Create the Payment record
        var payment = new Payment
        {
            IdempotencyKey = dto.IdempotencyKey,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Status = "Pending",
            CustomerPhone = dto.Phone,
            CustomerEmail = dto.Email,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        // 4. Route to the correct provider
        try
        {
            if (dto.Method.Equals("MpesaSTKPush", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessMpesaStkPushAsync(payment, clientApp, dto);
            }
            else if (dto.Method.Equals("StripeCard", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotImplementedException("Stripe integration coming soon.");
            }
            else
            {
                throw new Exception($"Unsupported payment method: {dto.Method}");
            }
        }
        catch (Exception ex)
        {
            payment.Status = "Failed";
            payment.FailureReason = ex.Message;
            logger.LogError(ex, "❌ Payment {PaymentId} failed: {Error}", payment.Id, ex.Message);
            await context.SaveChangesAsync();
            throw;
        }

        return payment;
    }

    private async Task ProcessMpesaStkPushAsync(Payment payment, ClientApp clientApp, CreatePaymentDto dto)
    {
        if (string.IsNullOrEmpty(clientApp.DarajaConsumerKey))
            throw new Exception("Daraja is not configured for this Client App.");

        // Decrypt the keys
        var consumerKey = encryptionService.Decrypt(clientApp.DarajaConsumerKey);
        var consumerSecret = encryptionService.Decrypt(clientApp.DarajaConsumerSecret);
        // One-off diagnostic — run this against the affected ClientApp, don't leave it in
        var passKey = encryptionService.Decrypt(clientApp.DarajaPassKey);
        Console.WriteLine($"Length: {passKey.Length}"); // should be 64
        Console.WriteLine($"Value:  {passKey}");
        var shortCode = clientApp.DarajaShortCode;
        var callbackUrl = clientApp.DarajaCallbackUrl;

        logger.LogInformation("🔑 Decrypted Daraja keys successfully");
        logger.LogInformation("📱 ShortCode: {ShortCode}", shortCode);
        logger.LogInformation("🔗 Callback URL: {CallbackUrl}", callbackUrl);

        var baseUrl = clientApp.Environment == "Production" 
            ? "https://api.safaricom.co.ke" 
            : "https://sandbox.safaricom.co.ke";

        // 1. Get Access Token
        var accessToken = await darajaService.GetAccessTokenAsync(consumerKey, consumerSecret, baseUrl);
        logger.LogInformation("✅ Got Daraja access token");

        // 🔑 KEY FIX: Use DateTime.Now (local time) instead of DateTime.UtcNow!
        // This matches your reference code and is likely why it was failing
        
        var  a= "174379";
        var b = "bfb279f9aa9bdbcf158e97dd71a467cd2e0c893059b10f78e6b72ada1ed2c919";
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{a}{b}{timestamp}"));

        // 🔑 KEY FIX: Robust phone number formatting (from your reference code)
        var phoneNumber = dto.Phone?.Trim() ?? throw new Exception("Phone number is required for M-Pesa");
        if (phoneNumber.StartsWith("+")) phoneNumber = phoneNumber.Substring(1);
        if (phoneNumber.StartsWith("0")) phoneNumber = "254" + phoneNumber.Substring(1);
        if (!phoneNumber.StartsWith("254")) phoneNumber = "254" + phoneNumber;

        logger.LogInformation("📞 Formatted Phone: {Phone}", phoneNumber);
        logger.LogInformation("💰 Amount: {Amount}", payment.Amount);

        // 2. Build the STK Push Request
        var stkRequest = new DarajaStkPushRequest
        {
            BusinessShortCode = shortCode,
            Password = password,
            Timestamp = timestamp,
            TransactionType = "CustomerPayBillOnline",
            Amount = (int)payment.Amount,
            PartyA = phoneNumber,
            PartyB = shortCode,
            PhoneNumber = phoneNumber,
            CallBackURL = callbackUrl,
            AccountReference = payment.Id.ToString().Substring(0, Math.Min(12, payment.Id.ToString().Length)),
            TransactionDesc = payment.Description.Length > 20 ? payment.Description.Substring(0, 20) : payment.Description
        };

        logger.LogInformation("🚀 Sending STK Push request to Daraja...");

        // 3. Send the STK Push
        var response = await darajaService.SendStkPushAsync(baseUrl, accessToken, stkRequest);

        // 4. Update the payment
        if (response.ResponseCode == "0")
        {
            payment.Status = "Processing";
            payment.ProviderReference = response.CheckoutRequestID;
            logger.LogInformation("✅ STK Push sent successfully! CheckoutRequestID: {Id}", response.CheckoutRequestID);
        }
        else
        {
            payment.Status = "Failed";
            payment.FailureReason = response.ResponseDescription;
            logger.LogWarning("❌ STK Push rejected: {Reason}", response.ResponseDescription);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid id)
    {
        return await context.Payments.FindAsync(id);
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
    {
        return await context.Payments.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }
}