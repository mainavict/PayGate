using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using PayGate.DTOs.Daraja;

namespace PayGate.Controllers;

[ApiController]
[Route("webhooks")]
public class WebhooksController(AppDbContext context, ILogger<WebhooksController> logger) : ControllerBase
{
    [HttpPost("daraja")]
    public async Task<IActionResult> HandleDarajaCallback([FromBody] DarajaCallbackDto callback)
    {
        // 1. Log the raw payload (Crucial for debugging if something goes wrong)
        logger.LogInformation("Daraja Callback Received: {@Callback}", callback);

        if (callback?.Body?.stkCallback == null)
        {
            logger.LogWarning("Invalid Daraja callback payload.");
            return BadRequest("Invalid payload.");
        }

        var stkCallback = callback.Body.stkCallback;
        var checkoutRequestId = stkCallback.CheckoutRequestID;

        // 2. Find the payment in our database using the CheckoutRequestID
        // (Remember, we saved this as the ProviderReference when we sent the STK Push!)
        var payment = await context.Payments
            .FirstOrDefaultAsync(p => p.ProviderReference == checkoutRequestId);

        if (payment == null)
        {
            logger.LogWarning("Payment not found for CheckoutRequestID: {Id}", checkoutRequestId);
            // We return 200 OK anyway so Daraja doesn't keep retrying and spamming us
            return Ok("Payment not found, but acknowledged."); 
        }

        // 3. Update payment status based on ResultCode
        if (stkCallback.ResultCode == 0)
        {
            // SUCCESS!
            payment.Status = "Completed";
            payment.CompletedAt = DateTime.UtcNow;
            
            // Extract the M-Pesa Receipt Number (e.g., "NLJ71U8V9K")
            var receipt = stkCallback.CallbackMetadata?.Item?
                .FirstOrDefault(i => i.Name == "MpesaReceiptNumber")?.Value;
            
            if (!string.IsNullOrEmpty(receipt))
            {
                logger.LogInformation("✅ M-Pesa Payment Success! Receipt: {Receipt}", receipt);
                // You could add a 'ReceiptNumber' column to your Payment model to save this
            }
        }
        else
        {
            // FAILED (User cancelled, no money, etc.)
            payment.Status = "Failed";
            payment.FailureReason = stkCallback.ResultDesc ?? "Unknown failure";
            logger.LogWarning("❌ M-Pesa Payment Failed: {Reason}", payment.FailureReason);
        }

        // 4. Save the updated status to the database
        await context.SaveChangesAsync();

        // 5. ALWAYS return 200 OK to Daraja. 
        // If you return an error, Daraja will think you didn't get it and will retry multiple times!
        return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });
    }
}