namespace PayGate.DTOs;

public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    
    // Temporary: Until we build Auth, we pass the OwnerId manually
    public Guid OwnerId { get; set; } 
}