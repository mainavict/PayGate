using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // <-- 1. Add this using statement
using Microsoft.IdentityModel.Tokens;
using PayGate.Data;
using PayGate.DTOs;
using PayGate.Models;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

// 2. Add IConfiguration to the constructor
public class UserService(AppDbContext context, IConfiguration configuration) : IUserService
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

        if (!user.IsActive)
            throw new Exception("User account is deactivated.");

        var tokenHandler = new JwtSecurityTokenHandler();

        var secretKey = configuration["Jwt:SecretKey"] ?? "PayGateSuperSecretKeyForJWTGeneration2026!MustBeLongEnough";
        
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

        return new AuthResponseDto
        {
            Token = tokenHandler.WriteToken(new JwtSecurityToken(
                issuer: "PayGate",
                audience: "PayGateUsers",
                claims: new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName)
                },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes), // <-- 4. Use the dynamic key here
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true // New users are active by default
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