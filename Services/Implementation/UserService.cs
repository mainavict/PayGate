using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using PayGate.DTOs;
using PayGate.Models;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class UserService(AppDbContext context) : IUserService
{
    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
    {
        // 1. Check if email already exists
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new Exception("A user with this email already exists.");

        // 2. Create the user and HASH the password
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) // 🔒 SECURE HASH
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 3. Return the DTO (without the password)
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return null;

        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}