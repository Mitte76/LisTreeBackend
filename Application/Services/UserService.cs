using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : ToDto(user);
    }

    public async Task UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.Username = dto.Username;
        user.Email = dto.Email;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    private static UserDto ToDto(User user) => new UserDto
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email
    };
}
