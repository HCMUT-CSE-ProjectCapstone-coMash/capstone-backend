using Capstone.Application.Common;

namespace Capstone.Application.Services.Authentication;

public interface IAuthenticationService
{
    Task<Result<AuthResult>> Register(string employeeId, string fullName, string email, string phoneNumber, string gender, string dateOfBirth);

    Task<Result> UpdateUserImageKey(string userId, string imageKey);

    Task<Result<string>> EditEmployee(string id, string? fullName, string? gender, string? dateOfBirth, string? phoneNumber, string? email);

    Task<Result<string>> DeleteEmployee(string id);

    Task<Result<AuthResult>> Login(string email, string password);
    
    Task<Result<UserDto>> GetUserById(string userId);

    Task<Result<PaginatedResult<UserDto>>> GetAllEmployees(int page, int pageSize, string? search = null);
    
    Task<Result<string>> CreateEmployeeId();
}