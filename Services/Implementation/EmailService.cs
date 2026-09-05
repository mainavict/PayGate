using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayGate.DTOs;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class EmailService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailServices
{
    private const string ResendApiUrl = "https://api.resend.com/emails";

    public async Task<bool> SendEmailAsync(EmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.To))
        {
            logger.LogWarning("Email recipient address is missing or empty.");
            return false;
        }

        var apiKey = configuration["Resend:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("re_your_"))
        {
            logger.LogWarning("Resend:ApiKey is not configured or still has placeholder value. Email to {To} was skipped.", dto.To);
            return false;
        }

        var fromEmail = !string.IsNullOrWhiteSpace(dto.From)
            ? dto.From
            : configuration["Resend:FromEmail"] ?? "PayGate <onboarding@resend.dev>";

        var payload = new ResendEmailRequest
        {
            From = fromEmail,
            To = [dto.To],
            Subject = dto.Subject,
            Html = dto.HtmlBody ?? dto.Body ?? string.Empty,
            Text = dto.Body
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(payload);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Email successfully sent to {To} (Purpose: {Purpose})", dto.To, dto.Purpose ?? "General");
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to send email via Resend to {To}. Status: {StatusCode}, Error: {Error}",
                dto.To, response.StatusCode, errorBody);

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception encountered while sending email via Resend to {To}", dto.To);
            return false;
        }
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string purpose = "Password Reset")
    {
        var htmlContent = BuildOtpTemplate(otpCode, purpose);
        var subject = $"PayGate - Your {purpose} Code: {otpCode}";

        var emailDto = new EmailDto
        {
            To = toEmail,
            Subject = subject,
            HtmlBody = htmlContent,
            Body = $"Your PayGate verification code for {purpose} is: {otpCode}. It is valid for 10 minutes.",
            Purpose = purpose
        };

        return await SendEmailAsync(emailDto);
    }

    private static string BuildOtpTemplate(string otpCode, string purpose)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>Verification Code</title>
        </head>
        <body style="margin: 0; padding: 0; background-color: #0b0f19; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #e2e8f0;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color: #0b0f19; padding: 40px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" width="100%" style="max-width: 520px; background-color: #131b2e; border: 1px solid #1e293b; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.4);">
                  <!-- Header -->
                  <tr>
                    <td style="padding: 32px 32px 20px 32px; text-align: center; border-bottom: 1px solid #1e293b;">
                      <div style="display: inline-block; padding: 10px 18px; border-radius: 9999px; background: linear-gradient(135deg, #6366f1 0%, #3b82f6 100%); color: #ffffff; font-weight: 800; font-size: 18px; letter-spacing: -0.5px;">
                        PayGate
                      </div>
                      <h2 style="margin: 20px 0 6px 0; color: #f8fafc; font-size: 22px; font-weight: 700;">{purpose} Verification</h2>
                      <p style="margin: 0; color: #94a3b8; font-size: 14px;">Use the one-time code below to complete your verification.</p>
                    </td>
                  </tr>

                  <!-- OTP Section -->
                  <tr>
                    <td style="padding: 32px; text-align: center;">
                      <div style="background-color: #0f172a; border: 2px dashed #334155; border-radius: 12px; padding: 24px; margin-bottom: 24px;">
                        <span style="font-family: 'Courier New', Courier, monospace; font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #38bdf8; display: inline-block;">
                          {otpCode}
                        </span>
                      </div>
                      <p style="margin: 0 0 16px 0; color: #cbd5e1; font-size: 14px; line-height: 1.6;">
                        This code will expire in <strong style="color: #f8fafc;">10 minutes</strong>. Do not share this code with anyone.
                      </p>
                      <p style="margin: 0; color: #64748b; font-size: 12px; line-height: 1.5;">
                        If you did not request this code, you can safely ignore this email. Someone may have entered your email address by mistake.
                      </p>
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="padding: 20px 32px; background-color: #0d1322; border-top: 1px solid #1e293b; text-align: center;">
                      <p style="margin: 0; color: #475569; font-size: 12px;">
                        &copy; {DateTime.UtcNow.Year} PayGate Payment Gateway. All rights reserved.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }

    private sealed class ResendEmailRequest
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public List<string> To { get; set; } = [];

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("html")]
        public string Html { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }
    }
}