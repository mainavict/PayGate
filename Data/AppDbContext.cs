using Microsoft.EntityFrameworkCore;
using PayGate.Models;

namespace PayGate.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<ClientApp> ClientApps { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WebhookLog> WebhookLogs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => e.ProviderReference);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });

        // Tenant Configuration (One-to-Many with User)
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            
            entity.HasOne(t => t.Owner)
                  .WithMany(u => u.Tenants)
                  .HasForeignKey(t => t.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict); 
        });

        modelBuilder.Entity<ClientApp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ApiKeyHash).IsUnique();
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique(); 
        });

        modelBuilder.Entity<WebhookLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Provider);
            entity.HasIndex(e => e.ReceivedAt);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}