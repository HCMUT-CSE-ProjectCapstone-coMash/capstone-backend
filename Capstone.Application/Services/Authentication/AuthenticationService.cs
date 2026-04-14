using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Authentication;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Capstone.Application.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUsersRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IFileStorageProvider _fileStorageProvider;

    public AuthenticationService(
        IUsersRepository userRepository, 
        IPasswordHasher passwordHasher, 
        IDateTimeProvider dateTimeProvider, 
        IJwtTokenGenerator jwtTokenGenerator, 
        IFileStorageProvider fileStorageProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _dateTimeProvider = dateTimeProvider;
        _jwtTokenGenerator = jwtTokenGenerator;
        _fileStorageProvider = fileStorageProvider;
    }

    public async Task<Result<RegisterDto>> Register(
        string employeeId,
        string fullName, 
        string email, 
        string phoneNumber, 
        string gender, 
        string dateOfBirth,
        IFormFile? image)
    {
        var existing = await _userRepository.GetUserByEmail(email);

        if (existing != null)
        {
            return Result<RegisterDto>.Failure(new Error(AuthErrors.UserAlreadyExists.Code, AuthErrors.UserAlreadyExists.Description));
        }
        var password = "123456";
        var newUserID = Guid.NewGuid();
        string imageKey = "";
        string imageUrl = "";
        if (image != null)
        {
            var extension = Path.GetExtension(image.FileName);

            await _fileStorageProvider.UploadUserImageAsync(
                newUserID,
                image.OpenReadStream(),
                image.ContentType,
                extension
            );

            imageKey = $"users/{newUserID}{extension}";
            imageUrl = await _fileStorageProvider.GetImageUrlAsync(imageKey);
        }
        var user = new User
        {
            Id = newUserID,
            EmployeeId = employeeId,
            FullName = fullName,
            Email = email,
            Password = _passwordHasher.Hash(password),
            CreatedAt = _dateTimeProvider.UtcNow,
            Role = Roles.Employee,
            Status = UserStatus.Active,
            PhoneNumber = phoneNumber,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            ImageKey = imageKey
        }; 

        await _userRepository.AddUser(user);

        return Result<RegisterDto>.Success(new RegisterDto(
            user.Id,
            user.EmployeeId,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            _jwtTokenGenerator.GenerateToken(user.Id, user.FullName, user.Role),
            user.PhoneNumber,
            user.Gender,
            user.DateOfBirth,
            imageUrl
        ));
    }

    public async Task<Result<string>> CreateEmployeeId()
    {
        const string prefix = "NV";

        var maxNumber = await _userRepository.GetMaxEmployeeNumberAsync(prefix);
        var newId = $"{prefix}-{maxNumber + 1:D3}";

        return Result<string>.Success(newId);
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

    public async Task<Result<EmployeeDto>> GetUserById(string userId)
    {
        var user = await _userRepository.GetUserById(Guid.Parse(userId));

        if (user is null)
        {
            return Result<EmployeeDto>.Failure(new Error(AuthErrors.UserNotExisted.Code, AuthErrors.UserNotExisted.Description));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<EmployeeDto>.Failure(new Error(AuthErrors.UserDeleted.Code, AuthErrors.UserDeleted.Description));
        }

        return Result<EmployeeDto>.Success(new EmployeeDto(
            user.Id,
            user.EmployeeId ?? string.Empty,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            user.PhoneNumber,
            user.Gender,
            user.DateOfBirth,
            !string.IsNullOrWhiteSpace(user.ImageKey)
                ? await _fileStorageProvider.GetImageUrlAsync(user.ImageKey)
                : string.Empty
        ));
    }
    public async Task<Result<List<UserDto>>> GetAllEmployees()
    {
        var users = await _userRepository.GetEmployees();

        var result = users.Select(u => new UserDto(
            u.EmployeeId ?? string.Empty,
            u.FullName,
            u.Email,
            u.Role,
            u.PhoneNumber,
            u.Gender,
            u.DateOfBirth
        )).ToList();

        return Result<List<UserDto>>.Success(result);
    }
}