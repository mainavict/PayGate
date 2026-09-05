using Microsoft.AspNetCore.Mvc;
using PayGate.DTOs;
using PayGate.Services.Interfaces;

namespace PayGate.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/email")]
public class EmailsController(IEmailServices emailServices) : ControllerBase
{
    /// <summary>
    /// Send an email using Resend
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.To))
        {
            return BadRequest(new { message = "Recipient email address ('to') is required." });
        }

        if (string.IsNullOrWhiteSpace(dto.Subject))
        {
            return BadRequest(new { message = "Email subject is required." });
        }

        try
        {
            var sent = await emailServices.SendEmailAsync(dto);
            if (!sent)
            {
                return StatusCode(500, new { message = "Failed to send email. Check API key configuration or server logs." });
            }

            return Ok(new { success = true, message = "Email sent successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while sending email.", error = ex.Message });
        }
    }

    /// <summary>
    /// Send a branded OTP verification code email using Resend
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtpEmail([FromBody] SendOtpRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email is required." });
        }

        if (string.IsNullOrWhiteSpace(dto.Otp))
        {
            return BadRequest(new { message = "OTP code is required." });
        }

        try
        {
            var sent = await emailServices.SendOtpEmailAsync(dto.Email, dto.Otp, dto.Purpose ?? "Password Reset");
            if (!sent)
            {
                return StatusCode(500, new { message = "Failed to send OTP email. Check API key configuration or server logs." });
            }

            return Ok(new { success = true, message = "OTP email sent successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while sending OTP email.", error = ex.Message });
        }
    }
}
