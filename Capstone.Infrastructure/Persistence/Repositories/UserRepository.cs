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

    public async Task<(List<User> Items, int Total)> SearchEmployees(int page, int pageSize, string? search = null)
    {
        var query = _context.Users
            .Where(u => u.Role == "employee" && u.Status == "Active")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";
            query = query.Where(u =>
                EF.Functions.ILike(
                    EF.Functions.Unaccent(u.FullName),
                    EF.Functions.Unaccent(searchPattern))
                || EF.Functions.ILike(u.EmployeeId ?? string.Empty, searchPattern)
                || EF.Functions.ILike(u.Email, searchPattern)
            );
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> GetMaxEmployeeNumberAsync(string prefix)
    {
        var numberStrings = await _context.Users
            .Where(u => u.EmployeeId != null && u.EmployeeId.StartsWith(prefix + "-"))
            .Select(u => u.EmployeeId!.Substring(prefix.Length + 1))
            .ToListAsync();

        if (!numberStrings.Any()) return 0;

        return numberStrings
            .Max(n => int.TryParse(n, out var num) ? num : 0);
    }
}