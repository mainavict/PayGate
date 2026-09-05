namespace PayGate.DTOs;
 
public class EmailDto
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? HtmlBody { get; set; }
    public string? Purpose { get; set; }
    public string? From { get; set; }
}

public class SendOtpRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string Purpose { get; set; } = "Password Reset";
}