namespace PayGate.Models;

public class ClientApp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = "Sandbox"; // Sandbox or Production
    
    // The API Key (Hashed for storage)
    public string ApiKeyHash { get; set; } = string.Empty; 
    
    // Rate Limiting & Tracking
    public int RateLimitPerMinute { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }

    // --- Daraja Configuration (Encrypted) ---
    public string? DarajaConsumerKey { get; set; }
    public string? DarajaConsumerSecret { get; set; }
    public string? DarajaShortCode { get; set; }
    public string? DarajaPassKey { get; set; }
    public string? DarajaCallbackUrl { get; set; }
    
    // --- Stripe Configuration (Encrypted) ---
    public string? StripeSecretKey { get; set; }
    public string? StripePublishableKey { get; set; }
    public string? StripeWebhookSecret { get; set; }

    // Navigation
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
   
}