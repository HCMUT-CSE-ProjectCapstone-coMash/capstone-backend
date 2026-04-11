using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Authentication;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUsersRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthenticationService(IUsersRepository userRepository, IPasswordHasher passwordHasher, IDateTimeProvider dateTimeProvider, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _dateTimeProvider = dateTimeProvider;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResult>> Register(string fullName, string email, string password)
    {
        var existing = await _userRepository.GetUserByEmail(email);

        if (existing != null)
        {
            return Result<AuthResult>.Failure(new Error(AuthErrors.UserAlreadyExists.Code, AuthErrors.UserAlreadyExists.Description));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            Password = _passwordHasher.Hash(password),
            CreatedAt = _dateTimeProvider.UtcNow,
            Role = Roles.Employee,
            Status = UserStatus.Active
        };

        await _userRepository.AddUser(user);

        return Result<AuthResult>.Success(new AuthResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            _jwtTokenGenerator.GenerateToken(user.Id, user.FullName, user.Role)
        ));
    }

    public async Task<Result<AuthResult>> Login(string email, string password)
    {
        var user = await _userRepository.GetUserByEmail(email);

        if (user is null || !_passwordHasher.Verify(password, user.Password))
        {
            return Result<AuthResult>.Failure(new Error(AuthErrors.InvalidCredentials.Code, AuthErrors.InvalidCredentials.Description));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<AuthResult>.Failure(new Error(AuthErrors.UserDeleted.Code, AuthErrors.UserDeleted.Description));
        }

        return Result<AuthResult>.Success(new AuthResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            _jwtTokenGenerator.GenerateToken(user.Id, user.FullName, user.Role)
        ));
    }

    public async Task<Result<AuthResult>> GetUserById(string userId)
    {
        var user = await _userRepository.GetUserById(Guid.Parse(userId));

        if (user is null)
        {
            return Result<AuthResult>.Failure(new Error(AuthErrors.UserNotExisted.Code, AuthErrors.UserNotExisted.Description));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<AuthResult>.Failure(new Error(AuthErrors.UserDeleted.Code, AuthErrors.UserDeleted.Description));
        }

        return Result<AuthResult>.Success(new AuthResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            ""
        ));
    }
    public async Task<Result<List<UserResponse>>> GetAllEmployees()
    {
        var users = await _userRepository.GetEmployees();

        var result = users.Select(u => new UserResponse(
            u.Id,
            u.FullName,
            u.Email,
            u.Role,
            u.PhoneNumber,
            u.Gender,
            u.DateOfBirth
        )).ToList();

        return Result<List<UserResponse>>.Success(result);
    }
}