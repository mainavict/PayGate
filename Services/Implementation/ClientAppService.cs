using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using PayGate.DTOs;
using PayGate.Models;
using PayGate.Services.Interfaces;
using PayGate.Utils;

namespace PayGate.Services.Implementation;

public class ClientAppService(AppDbContext context, IEncryptionService encryptionService) : IClientAppService
{
    public async Task<ClientAppResponseDto> CreateClientAppAsync(CreateClientAppDto dto)
    {
        var tenant = await context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null) throw new Exception($"Tenant {dto.TenantId} not found.");

        var prefix = dto.Environment == "Production" ? "pg_live_" : "pg_test_";
        var plainTextApiKey = SecurityHelper.GenerateSecureApiKey(prefix);
        var apiKeyHash = SecurityHelper.HashApiKey(plainTextApiKey);

        var app = new ClientApp
        {
            Name = dto.Name, Environment = dto.Environment, TenantId = dto.TenantId,
            ApiKeyHash = apiKeyHash, RateLimitPerMinute = dto.RateLimitPerMinute,
            DarajaConsumerKey = encryptionService.Encrypt(dto.DarajaConsumerKey),
            DarajaConsumerSecret = encryptionService.Encrypt(dto.DarajaConsumerSecret),
            DarajaPassKey = encryptionService.Encrypt(dto.DarajaPassKey),
            DarajaShortCode = dto.DarajaShortCode, DarajaCallbackUrl = dto.DarajaCallbackUrl,
            StripeSecretKey = encryptionService.Encrypt(dto.StripeSecretKey),
            StripePublishableKey = dto.StripePublishableKey,
            StripeWebhookSecret = encryptionService.Encrypt(dto.StripeWebhookSecret)
        };

        context.ClientApps.Add(app);
        await context.SaveChangesAsync();

        return new ClientAppResponseDto
        {
            Id = app.Id, Name = app.Name, Environment = app.Environment,
            ApiKey = plainTextApiKey, RateLimitPerMinute = app.RateLimitPerMinute,
            IsActive = app.IsActive, CreatedAt = app.CreatedAt,
            IsDarajaConfigured = !string.IsNullOrEmpty(app.DarajaConsumerKey),
            IsStripeConfigured = !string.IsNullOrEmpty(app.StripeSecretKey)
        };
    }

    public async Task<IEnumerable<ClientAppResponseDto>> GetClientAppsByTenantIdAsync(Guid tenantId)
    {
        var apps = await context.ClientApps.Where(c => c.TenantId == tenantId).ToListAsync();
        return apps.Select(app => new ClientAppResponseDto
        {
            Id = app.Id, Name = app.Name, Environment = app.Environment,
            ApiKey = "••••••••••••••••", RateLimitPerMinute = app.RateLimitPerMinute,
            IsActive = app.IsActive, CreatedAt = app.CreatedAt,
            IsDarajaConfigured = !string.IsNullOrEmpty(app.DarajaConsumerKey),
            IsStripeConfigured = !string.IsNullOrEmpty(app.StripeSecretKey)
        });
    }
}