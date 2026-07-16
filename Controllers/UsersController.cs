using Microsoft.AspNetCore.Mvc;
using PayGate.DTOs;
using PayGate.Services.Interfaces;

namespace PayGate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Register a new user (Admin/Merchant)
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticate user and return JWT Token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await userService.LoginAsync(dto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = "User not found" });
        return Ok(user);
    }
}