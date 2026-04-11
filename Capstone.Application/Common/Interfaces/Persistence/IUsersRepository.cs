using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IUsersRepository
{
    Task<User?> GetUserByEmail(string email);
    Task AddUser(User user);
    Task<User?> GetUserById(Guid userId);
    Task<List<User>> GetEmployees();
}