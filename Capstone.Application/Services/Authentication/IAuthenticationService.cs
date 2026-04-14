using Capstone.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Capstone.Application.Services.Authentication;

public interface IAuthenticationService
{
    Task<Result<string>> Register(string employeeId, string fullName, string email, string phoneNumber, string gender, string dateOfBirth);
    Task<Result<AuthResult>> Login(string email, string password);
    Task<Result<EmployeeDto>> GetUserById(string userId);
    Task<Result<List<UserDto>>> GetAllEmployees();
    Task<Result<string>> CreateEmployeeId();
}