namespace PayGate.Models;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<ClientApp> ClientApps { get; set; } = new List<ClientApp>();
}