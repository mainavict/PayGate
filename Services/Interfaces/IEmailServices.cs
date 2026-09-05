using PayGate.DTOs;

namespace PayGate.Services.Interfaces;

public interface IEmailServices
{
    Task<bool> SendEmailAsync(EmailDto dto);
    Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string purpose = "Password Reset");
}
