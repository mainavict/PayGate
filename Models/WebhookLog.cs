namespace PayGate.Models;

public class WebhookLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Provider { get; set; } = string.Empty; 
    public string RawPayload { get; set; } = string.Empty; 
    public string Status { get; set; } = "Pending"; 
    public string? ErrorMessage { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}