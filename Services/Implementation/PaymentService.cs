using PayGate.Data;
using PayGate.DTOs;
using PayGate.DTOs.Daraja;
using PayGate.Models;
using PayGate.Services.Interfaces;
using  Microsoft.EntityFrameworkCore;

namespace PayGate.Services.Implementation;

public class PaymentService(
    AppDbContext context, 
    IEncryptionService encryptionService, 
    IDarajaService darajaService,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<Payment> ProcessPaymentAsync(CreatePaymentDto dto, Guid clientAppId)
    {
        
        if (string.IsNullOrWhiteSpace(dto.IdempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey is required to prevent duplicate charges.");
        }
        // 1. Check for duplicates (Idempotency)
        var existingPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == dto.IdempotencyKey);
            
        if (existingPayment != null)
        {
            logger.LogWarning("Duplicate payment request detected for key: {Key}", dto.IdempotencyKey);
            return existingPayment;
        }

        // 2. Get the Client App to find the payment credentials
        var clientApp = await context.ClientApps.FindAsync(clientAppId)
            ?? throw new Exception($"Client App {clientAppId} not found.");

        if (!clientApp.IsActive)
            throw new Exception("This Client App is deactivated.");

        // 3. Create the Payment record in the database (Status: Pending)
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

        // 4. Route to the correct provider based on the Method
        try
        {
            if (dto.Method.Equals("MpesaSTKPush", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessMpesaStkPushAsync(payment, clientApp, dto);
            }
            else if (dto.Method.Equals("StripeCard", StringComparison.OrdinalIgnoreCase))
            {
                // We will add Stripe logic here later!
                throw new NotImplementedException("Stripe integration coming soon.");
            }
            else
            {
                throw new Exception($"Unsupported payment method: {dto.Method}");
            }
        }
        catch (Exception ex)
        {
            // If the provider call fails, mark the payment as failed
            payment.Status = "Failed";
            payment.FailureReason = ex.Message;
            await context.SaveChangesAsync();
            
            logger.LogError(ex, "Payment {PaymentId} failed", payment.Id);
            throw;
        }

        return payment;
    }

    // --- Helper method for M-Pesa ---
    private async Task ProcessMpesaStkPushAsync(Payment payment, ClientApp clientApp, CreatePaymentDto dto)
    {
        if (string.IsNullOrEmpty(clientApp.DarajaConsumerKey))
            throw new Exception("Daraja is not configured for this Client App.");

        // Decrypt the keys
        var consumerKey = encryptionService.Decrypt(clientApp.DarajaConsumerKey);
        var consumerSecret = encryptionService.Decrypt(clientApp.DarajaConsumerSecret);
        var passKey = encryptionService.Decrypt(clientApp.DarajaPassKey);
        var shortCode = clientApp.DarajaShortCode;
        var callbackUrl = clientApp.DarajaCallbackUrl;

        // Determine environment (Sandbox vs Production)
        var baseUrl = clientApp.Environment == "Production" 
            ? "https://api.safaricom.co.ke" 
            : "https://sandbox.safaricom.co.ke";

        // 1. Get Access Token
        var accessToken = await darajaService.GetAccessTokenAsync(consumerKey, consumerSecret, baseUrl);

        // 2. Generate Password and Timestamp
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{shortCode}{passKey}{timestamp}"));

        // 3. Build the STK Push Request
        var stkRequest = new DarajaStkPushRequest
        {
            BusinessShortCode = shortCode,
            Password = password,
            Timestamp = timestamp,
            Amount = (int)payment.Amount, // Daraja expects an integer
            PartyA = dto.Phone,
            PartyB = shortCode,
            PhoneNumber = dto.Phone,
            CallBackURL = callbackUrl,
            AccountReference = payment.Id.ToString().Substring(0, 12), // Max 12 chars
            TransactionDesc = payment.Description
        };

        // 4. Send the STK Push
        var response = await darajaService.SendStkPushAsync(baseUrl, accessToken, stkRequest);

        // 5. Update the payment with the provider reference
        if (response.ResponseCode == "0") // "0" means success in Daraja
        {
            payment.Status = "Processing"; // Waiting for the user to enter their PIN
            payment.ProviderReference = response.CheckoutRequestID;
        }
        else
        {
            payment.Status = "Failed";
            payment.FailureReason = response.ResponseDescription;
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