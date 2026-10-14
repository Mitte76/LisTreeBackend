using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LisTreeBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        try
        {
            await _userService.UpdateAsync(id, dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Application.Exceptions.DuplicateUsernameException)
        {
            return Conflict("Användarnamnet finns redan.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        try
        {
            var created = await _userService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
        }
        catch (Application.Exceptions.DuplicateUsernameException)
        {
            return Conflict("Användarnamnet finns redan.");
        }
    }
}
