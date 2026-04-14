using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IUsersRepository
{
    Task<User?> GetUserByEmail(string email);
    Task AddUser(User user);
    Task UpdateUser(User user);
    Task DeleteUserAsync(Guid userId);
    Task<User?> GetUserById(Guid userId);
    Task<List<User>> GetEmployees();
    Task<(List<User> Items, int Total)> SearchEmployees(int page, int pageSize, string? search = null);
    Task<int> GetMaxEmployeeNumberAsync(string prefix);
}