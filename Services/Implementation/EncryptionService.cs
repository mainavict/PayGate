using Microsoft.AspNetCore.DataProtection;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class EncryptionService(IDataProtectionProvider provider) : IEncryptionService
{
    private readonly IDataProtector _protector = provider.CreateProtector("PayGate.TenantSecrets.v1");

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        return _protector.Unprotect(cipherText);
    }
}