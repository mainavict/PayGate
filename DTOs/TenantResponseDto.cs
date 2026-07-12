namespace PayGate.DTOs;

public class TenantResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ClientAppResponseDto> ClientApps { get; set; } = new();
}