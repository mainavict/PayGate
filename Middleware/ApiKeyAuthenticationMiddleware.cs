using PayGate.Data;
using PayGate.Utils; // Assuming SecurityHelper is here
using Microsoft.EntityFrameworkCore;

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
        // ✅ NEW (Correct)
// context.Request.Path is a PathString, which has the StartsWithSegments method!
        var path = context.Request.Path; 

        if (path.StartsWithSegments("/scalar") || 
            path.StartsWithSegments("/openapi") ||
            path.StartsWithSegments("/postman") ||
            path.StartsWithSegments("/api/health") ||
            path.StartsWithSegments("/webhooks") || 
            path.StartsWithSegments("/api/users") || 
            path.StartsWithSegments("/api/ClientApps")) 
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
        // Note: extractedApiKey is a StringValues, so we call .ToString()
        var hashedKey = SecurityHelper.HashApiKey(extractedApiKey.ToString());

        // 4. Look up the Client App in the database
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

        // 5. Attach the ClientAppId and OwnerId to the HttpContext
        // 🔥 UPDATED: No more TenantId. We now use OwnerId directly.
        // This allows our Controllers and Services to know exactly which User/App is making the request.
        context.Items["ClientAppId"] = clientApp.Id;
        context.Items["OwnerId"] = clientApp.OwnerId; 

        // 6. Update last used time (Optional, but excellent for analytics and rate limiting)
        clientApp.LastUsedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        // 7. Continue to the next middleware/controller
        await _next(context);
    }
}