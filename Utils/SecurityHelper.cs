using System.Security.Cryptography;
using System.Text;

namespace PayGate.Utils;

public static class SecurityHelper
{
    public static string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(bytes);
    }
    
    public static string GenerateSecureApiKey(string prefix)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var base64 = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
        return $"{prefix}{base64}";
    }
}