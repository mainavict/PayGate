using Microsoft.AspNetCore.Mvc;
using PayGate.DTOs;
using PayGate.Services.Interfaces;

namespace PayGate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
    {
        var tenant = await tenantService.CreateTenantAsync(dto);
        return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTenants()
    {
        var tenants = await tenantService.GetAllTenantsAsync();
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenantById(Guid id)
    {
        var tenant = await tenantService.GetTenantByIdAsync(id);
        if (tenant == null) return NotFound();
        return Ok(tenant);
    }
}