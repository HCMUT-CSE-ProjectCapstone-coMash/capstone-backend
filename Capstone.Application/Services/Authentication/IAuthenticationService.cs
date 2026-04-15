using Capstone.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Capstone.Application.Services.Authentication;

public interface IAuthenticationService
{
    Task<Result<string>> Register(string employeeId, string fullName, string email, string phoneNumber, string gender, string dateOfBirth);
    Task<Result> UpdateUserImageKey(string userId, string imageKey);
    Task<Result<UserDto>> EditEmployee(string id, string? fullName, string? gender, string? dateOfBirth, string? phoneNumber, string? email);
    Task<Result<string>> DeleteEmployee(string id);
    Task<Result<AuthResult>> Login(string email, string password);
    Task<Result<UserDto>> GetUserById(string userId);
    Task<Result<List<UserDto>>> GetAllEmployees();
    Task<Result<PaginatedResult<UserDto>>> SearchEmployees(int currentPage, int pageSize, string? search = null);
    Task<Result<string>> CreateEmployeeId();
}