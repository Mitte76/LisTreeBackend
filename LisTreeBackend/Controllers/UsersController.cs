using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LisTreeBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User updatedUser)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;

        _userRepository.Update(user);

        try
        {
            await _userRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return Conflict("Användarnamnet finns redan.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        await _userRepository.AddAsync(user);

        try
        {
            await _userRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return Conflict("Användarnamnet finns redan.");
        }

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
