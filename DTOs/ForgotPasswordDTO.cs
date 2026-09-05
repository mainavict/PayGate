namespace PayGate.DTOs;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public  class ConfrimOTPDto
{
    public string Email { get; set; } = string.Empty;
    public string OTP { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string OTP { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
