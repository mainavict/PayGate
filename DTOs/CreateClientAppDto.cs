namespace PayGate.DTOs;

public class CreateClientAppDto
{
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = "Sandbox";
    public Guid OwnerId { get; set; }
    public int RateLimitPerMinute { get; set; } = 100;

    public string? DarajaConsumerKey { get; set; }
    public string? DarajaConsumerSecret { get; set; }
    public string? DarajaShortCode { get; set; }
    public string? DarajaPassKey { get; set; }
    public string? DarajaCallbackUrl { get; set; }
    
    public string? StripeSecretKey { get; set; }
    public string? StripePublishableKey { get; set; }
    public string? StripeWebhookSecret { get; set; }
}