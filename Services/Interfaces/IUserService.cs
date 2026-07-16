using PayGate.DTOs;
using PayGate.Models;

namespace PayGate.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    
    // We will need this later for the Login/Auth system
    Task<User?> GetUserByEmailAsync(string email); 
    
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}