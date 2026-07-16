using PayGate.DTOs;

namespace PayGate.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
}