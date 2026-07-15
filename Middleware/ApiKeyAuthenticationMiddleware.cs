using PayGate.Data;
using PayGate.Utils;
using System.Security.Cryptography;
using System.Text;
using  Microsoft.EntityFrameworkCore;

namespace PayGate.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        
        var path = context.Request.Path.Value ?? string.Empty; 
        // 1. Skip authentication for development tools and health checks
        if (context.Request.Path.StartsWithSegments("/scalar") || 
            context.Request.Path.StartsWithSegments("/openapi") ||
            context.Request.Path.StartsWithSegments("/postman")||
            context.Request.Path.StartsWithSegments("/api/health")||
            context.Request.Path.StartsWithSegments("/webhooks")||
            context.Request.Path.StartsWithSegments("/api/users")||
            context.Request.Path.StartsWithSegments("/api/Tenants") ||
            context.Request.Path.StartsWithSegments("/api/ClientApps")   
            )
            
        {
            await _next(context);
            return;
        } 

        // 2. Extract the API Key from the header
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Missing API Key in X-API-Key header.");
            return;
        }

        // 3. Hash the incoming key to compare with the database
        var hashedKey = SecurityHelper.HashApiKey(extractedApiKey);

        // 4. Look up the Client App in the database
        // Note: We use a service locator pattern here for simplicity in middleware, 
        // or we can inject the DB context directly.
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var clientApp = await dbContext.ClientApps
            .FirstOrDefaultAsync(c => c.ApiKeyHash == hashedKey && c.IsActive);

        if (clientApp == null)
        {
            _logger.LogWarning("Invalid or inactive API Key used: {Key}", extractedApiKey);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid or inactive API Key.");
            return;
        }

        // 5. Attach the ClientAppId and TenantId to the HttpContext
        // This allows our Controllers and Services to know who is making the request.
        context.Items["ClientAppId"] = clientApp.Id;
        context.Items["TenantId"] = clientApp.TenantId;

        // 6. Update last used time (Optional, good for analytics)
        clientApp.LastUsedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        // 7. Continue to the next middleware/controller
        await _next(context);
    }
}