using Microsoft.AspNetCore.Mvc;
using PayGate.DTOs;
using PayGate.Services.Interfaces;

namespace PayGate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Creates a new user (e.g., Victor the Admin) and securely hashes their password.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await userService.CreateUserAsync(dto);
            
            // Return 201 Created and provide the location to fetch the user later
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            // Catches duplicate email errors or other validation issues
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific user by their ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        
        if (user == null) 
            return NotFound(new { message = "User not found" });
            
        return Ok(user);
    }
}