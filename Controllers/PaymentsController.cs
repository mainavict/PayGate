using Microsoft.AspNetCore.Mvc;
using PayGate.DTOs;
using PayGate.Services.Interfaces;
using  PayGate.Models;


namespace PayGate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        // 1. Get the ClientAppId that the Middleware attached to the context
        if (!HttpContext.Items.TryGetValue("ClientAppId", out var clientAppIdObj) || clientAppIdObj == null)
        {
            return Unauthorized(new { message = "Authentication failed." });
        }

        var clientAppId = Guid.Parse(clientAppIdObj.ToString());

        try
        {
            // 2. Process the payment
            var payment = await paymentService.ProcessPaymentAsync(dto, clientAppId);
            
            // 3. Return the result (201 Created)
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }
        catch (ArgumentException ex)
        {
            // This catches our strict Idempotency Key check
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing the payment.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> GetPayment(Guid id)
    {
        var payment = await paymentService.GetPaymentByIdAsync(id);
        if (payment == null) return NotFound();
        return Ok(payment);
    }
}