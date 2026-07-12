using PayGate.DTOs;

namespace PayGate.Services.Interfaces;

public interface ITenantService
{
    Task<TenantResponseDto> CreateTenantAsync(CreateTenantDto dto);
    Task<IEnumerable<TenantResponseDto>> GetAllTenantsAsync();
    Task<TenantResponseDto?> GetTenantByIdAsync(Guid id);
}