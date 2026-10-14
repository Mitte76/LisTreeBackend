using Application.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Application.Exceptions;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Translate Postgres unique constraint violation to application-level exception
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                throw new DuplicateUsernameException("Användarnamnet finns redan.", ex);
            }

            throw;
        }
    }
}
