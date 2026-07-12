using Microsoft.AspNetCore.Mvc;
using PayGate.DTOs;
using PayGate.Services.Interfaces;

namespace PayGate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientAppsController(IClientAppService clientAppService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ClientAppResponseDto>> CreateClientApp([FromBody] CreateClientAppDto dto)
    {
        try
        {
            var clientApp = await clientAppService.CreateClientAppAsync(dto);
            return Ok(new { message = "Client app created. Save this API key immediately.", data = clientApp });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult<IEnumerable<ClientAppResponseDto>>> GetClientAppsByTenant(Guid tenantId)
    {
        var clientApps = await clientAppService.GetClientAppsByTenantIdAsync(tenantId);
        return Ok(clientApps);
    }
}