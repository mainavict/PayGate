namespace PayGate.DTOs;

public class CreatePaymentDto
{
    public string IdempotencyKey { get; set; } = string.Empty; // Prevents duplicate charges
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string Method { get; set; } = string.Empty; // "MpesaSTKPush", "StripeCard", etc.
    public string? Phone { get; set; } // Required for M-Pesa
    public string? Email { get; set; }
    public string Description { get; set; } = string.Empty;
}