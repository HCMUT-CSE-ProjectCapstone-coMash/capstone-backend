using Capstone.Application.Common;

namespace Capstone.Application.Services.Authentication;

public interface IAuthenticationService
{
    Task<Result<AuthResult>> Register(string fullName, string email, string password);
    Task<Result<AuthResult>> Login(string email, string password);
}