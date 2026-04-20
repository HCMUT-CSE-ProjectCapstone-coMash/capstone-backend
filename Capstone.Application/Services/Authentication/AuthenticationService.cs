using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Authentication;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.FileStorageService;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUsersRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISaleOrdersRepository _saleOrderRepository;
    private readonly IProductsOrdersRepository _productsOrdersRepository;

    public AuthenticationService(
        IUsersRepository userRepository, 
        IPasswordHasher passwordHasher, 
        IDateTimeProvider dateTimeProvider, 
        IJwtTokenGenerator jwtTokenGenerator, 
        IFileStorageService fileStorageService,
        ISaleOrdersRepository saleOrdersRepository,
        IProductsOrdersRepository productsOrdersRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _dateTimeProvider = dateTimeProvider;
        _jwtTokenGenerator = jwtTokenGenerator;
        _fileStorageService = fileStorageService;
        _saleOrderRepository = saleOrdersRepository;
        _productsOrdersRepository = productsOrdersRepository;

    }

    public async Task<Result<AuthResult>> Register(
        string employeeId,
        string fullName, 
        string email, 
        string phoneNumber, 
        string gender, 
        string dateOfBirth)
    {
        var existing = await _userRepository.GetUserByEmail(email);

        if (existing != null)
        {
            return Result<AuthResult>.Failure(new Error(AuthErrors.UserAlreadyExists.Code, AuthErrors.UserAlreadyExists.Description));
        }

        var password = "123456";

        var user = new User
        {
            Id =  Guid.NewGuid(),
            EmployeeId = employeeId,
            FullName = fullName,
            Email = email,
            Password = _passwordHasher.Hash(password),
            CreatedAt = _dateTimeProvider.UtcNow,
            Role = Roles.Employee,
            Status = UserStatus.Active,
            PhoneNumber = phoneNumber,
            Gender = gender,
            DateOfBirth = DateOnly.Parse(dateOfBirth),
        }; 

        await _userRepository.AddUser(user);

        return Result<AuthResult>.Success(new AuthResult(
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
            string.Empty
        ));
    }

    public async Task<Result> UpdateUserImageKey(string userId, string imageKey)
    {
        var user = await _userRepository.GetUserById(Guid.Parse(userId));

        if (user == null)
            return Result.Failure(new Error("UserNotFound", "User not found."));

        user.ImageKey = imageKey;

        await _userRepository.UpdateUser(user);

        return Result.Success();
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
        var imageUrl = "";
        if (!string.IsNullOrEmpty(user.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(user.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
        }

        return Result<AuthResult>.Success(new AuthResult(
            user.Id,
            user.EmployeeId ?? string.Empty,
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

    public async Task<Result<UserDto>> GetUserById(string id)
    {
        var user = await _userRepository.GetUserById(Guid.Parse(id));

        if (user is null)
        {
            return Result<UserDto>.Failure(new Error(AuthErrors.UserNotExisted.Code, AuthErrors.UserNotExisted.Description));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<UserDto>.Failure(new Error(AuthErrors.UserDeleted.Code, AuthErrors.UserDeleted.Description));
        }

        var imageUrl = "";
        if (!string.IsNullOrEmpty(user.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(user.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
        }

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.EmployeeId ?? string.Empty,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            user.PhoneNumber,
            user.Gender,
            user.DateOfBirth,
            imageUrl
        ));
    }
    public async Task<Result<PaginatedResult<UserDto>>> GetAllEmployees(int page, int pageSize, string? search = null)
    {
        var (employees, total) = await _userRepository.GetEmployees(page, pageSize, search);

        var userDtos = new List<UserDto>();

        foreach (var employee in employees)
        {
            var imageUrl = "";
            if (!string.IsNullOrEmpty(employee.ImageKey))
            {
                var imageResult = await _fileStorageService.GetImageUrlAsync(employee.ImageKey);
                imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
            }

            userDtos.Add(new UserDto(
                employee.Id,
                employee.EmployeeId ?? string.Empty,
                employee.FullName,
                employee.Email,
                employee.Role,
                employee.CreatedAt,
                employee.PhoneNumber,
                employee.Gender,
                employee.DateOfBirth,
                imageUrl
            ));
        }
        
        return Result<PaginatedResult<UserDto>>.Success(
            new PaginatedResult<UserDto>(userDtos, total));
    }

    public async Task<Result<string>> EditEmployee(
        string id,
        string? fullName,
        string? gender,
        string? dateOfBirth,
        string? phoneNumber,
        string? email
    )
    {
        var user = await _userRepository.GetUserById(Guid.Parse(id));

        if (user is null)
        {
            return Result<string>.Failure(new Error("NotFound", "User not found."));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<string>.Failure(new Error(AuthErrors.UserDeleted.Code, AuthErrors.UserDeleted.Description));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            user.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(fullName))
            user.FullName = fullName;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            user.PhoneNumber = phoneNumber;

        if (!string.IsNullOrWhiteSpace(gender))
            user.Gender = gender;

        if (!string.IsNullOrWhiteSpace(dateOfBirth))
            user.DateOfBirth = DateOnly.Parse(dateOfBirth);

        await _userRepository.UpdateUser(user);

        return Result<string>.Success(user.FullName);
    }

    public async Task<Result<string>> DeleteEmployee(string id)
    {
        var user = await _userRepository.GetUserById(Guid.Parse(id));
        if (user == null)
        {
            return Result<string>.Failure(new Error("NotFound", "User not found."));
        }

        var isInSaleOrder = await _saleOrderRepository.ExistsByEmployeeId(user.Id);
        var isInProductsOrder = await _productsOrdersRepository.ExistsByEmployeeId(user.Id);

        if (isInSaleOrder || isInProductsOrder)
        {
            user.Status = UserStatus.Deleted;
            await _userRepository.UpdateUser(user);
        }
        else
        {
            await _userRepository.DeleteUserAsync(user.Id);
        }

        return Result<string>.Success(user.FullName);
    }
}