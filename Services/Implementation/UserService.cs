using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using PayGate.DTOs;
using PayGate.Models;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class UserService(AppDbContext context) : IUserService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
            throw new Exception("Invalid email or password.");
 
        // Verify the password
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new Exception("Invalid email or password.");

       
        var  token = new JwtSecurityTokenHandler();

        return new AuthResponseDto
        {
            Token = token.WriteToken(new JwtSecurityToken(
                claims: new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName)
                },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyHere")), 
                    SecurityAlgorithms.HmacSha256)
            )),
            User = new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }
    
    

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