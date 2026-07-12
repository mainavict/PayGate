using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using PayGate.DTOs;
using PayGate.Models;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class TenantService(AppDbContext context) : ITenantService
{
    public async Task<TenantResponseDto> CreateTenantAsync(CreateTenantDto dto)
    {
        var tenant = new Tenant 
        { 
            Name = dto.Name, 
            ContactEmail = dto.ContactEmail,
            OwnerId = dto.OwnerId // Link to the User
        };
        
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        return MapToResponseDto(tenant);
    }

    public async Task<IEnumerable<TenantResponseDto>> GetAllTenantsAsync()
    {
        var tenants = await context.Tenants.Include(t => t.ClientApps).OrderByDescending(t => t.CreatedAt).ToListAsync();
        return tenants.Select(MapToResponseDto);
    }

    public async Task<TenantResponseDto?> GetTenantByIdAsync(Guid id)
    {
        var tenant = await context.Tenants.Include(t => t.ClientApps).FirstOrDefaultAsync(t => t.Id == id);
        return tenant == null ? null : MapToResponseDto(tenant);
    }

    private static TenantResponseDto MapToResponseDto(Tenant tenant)
    {
        return new TenantResponseDto
        {
            Id = tenant.Id, Name = tenant.Name, ContactEmail = tenant.ContactEmail,
            OwnerId = tenant.OwnerId, IsActive = tenant.IsActive, CreatedAt = tenant.CreatedAt,
            ClientApps = tenant.ClientApps.Select(c => new ClientAppResponseDto
            {
                Id = c.Id, Name = c.Name, Environment = c.Environment,
                ApiKey = "••••••••••••••••", RateLimitPerMinute = c.RateLimitPerMinute,
                IsActive = c.IsActive, CreatedAt = c.CreatedAt,
                IsDarajaConfigured = !string.IsNullOrEmpty(c.DarajaConsumerKey),
                IsStripeConfigured = !string.IsNullOrEmpty(c.StripeSecretKey)
            }).ToList()
        };
    }
}