using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly AppDbContext _context;

    public UsersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUser(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<User?> GetUserById(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<User>> GetEmployees()
    {
        return await _context.Users
            .Where(u => u.Role == "employee" && u.Status == "Active")
            .ToListAsync();
    }

    public async Task<int> GetMaxEmployeeNumberAsync(string prefix)
    {
        var numberStrings = await _context.Users
            .Where(u => u.EmployeeId.StartsWith(prefix + "-"))
            .Select(u => u.EmployeeId.Substring(prefix.Length + 1))
            .ToListAsync();

        if (!numberStrings.Any()) return 0;

        return numberStrings
            .Max(n => int.TryParse(n, out var num) ? num : 0);
    }
}