namespace PayGate.DTOs;

public class ClientAppResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty; // Shown ONLY on creation
    public int RateLimitPerMinute { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Just show if they are configured, don't show the actual keys
    public bool IsDarajaConfigured { get; set; }
    public bool IsStripeConfigured { get; set; }
}