using PayGate.DTOs;

namespace PayGate.Services.Interfaces;

public interface IClientAppService
{
    Task<ClientAppResponseDto> CreateClientAppAsync(CreateClientAppDto dto);
    Task<IEnumerable<ClientAppResponseDto>> GetClientAppsByTenantIdAsync(Guid tenantId);
}