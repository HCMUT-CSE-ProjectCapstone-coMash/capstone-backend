using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Authentication;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.FileStorageService;
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

    public async Task<Result<string>> Register(
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
            return Result<string>.Failure(new Error(AuthErrors.UserAlreadyExists.Code, AuthErrors.UserAlreadyExists.Description));
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
            DateOfBirth = dateOfBirth,
        }; 

        await _userRepository.AddUser(user);

        return Result<string>.Success(user.Id.ToString());
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

        return Result<AuthResult>.Success(new AuthResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            _jwtTokenGenerator.GenerateToken(user.Id, user.FullName, user.Role)
        ));
    }

    public async Task<Result<EmployeeDto>> GetUserById(string id)
    {
        var user = await _userRepository.GetUserById(Guid.Parse(id));

        if (user is null)
        {
            return Result<EmployeeDto>.Failure(new Error(AuthErrors.UserNotExisted.Code, AuthErrors.UserNotExisted.Description));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<EmployeeDto>.Failure(new Error(AuthErrors.UserDeleted.Code, AuthErrors.UserDeleted.Description));
        }

        var imageUrl = "";
        if (!string.IsNullOrEmpty(user.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(user.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
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
            imageUrl
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

    public async Task<Result<PaginatedResult<UserDto>>> SearchEmployees(int currentPage, int pageSize, string? search = null)
    {
        if (currentPage <= 0) currentPage = 1;
        if (pageSize <= 0) pageSize = 10;

        var (users, total) = await _userRepository.SearchEmployees(currentPage, pageSize, search);

        var items = users.Select(u => new UserDto(
            u.EmployeeId ?? string.Empty,
            u.FullName,
            u.Email,
            u.Role,
            u.PhoneNumber,
            u.Gender,
            u.DateOfBirth
        )).ToList();

        return Result<PaginatedResult<UserDto>>.Success(new PaginatedResult<UserDto>(items, total));
    }
    public async Task<Result<EmployeeDto>> EditEmployee(
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
            return Result<EmployeeDto>.Failure(new Error("NotFound", "User not found."));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<EmployeeDto>.Failure(new Error(AuthErrors.UserDeleted.Code, AuthErrors.UserDeleted.Description));
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
            user.DateOfBirth = dateOfBirth;

        await _userRepository.UpdateUser(user);

        var imageUrl = string.Empty;
        if (!string.IsNullOrEmpty(user.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(user.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : string.Empty;
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
            imageUrl
        ));
    }


    public async Task<Result<string>> DeleteEmployee(string id)
    {
        var user = await _userRepository.GetUserById(Guid.Parse(id));
        if (user == null)
        {
            return Result<string>.Failure(new Error("NotFound", "User not found."));
        }
        else if (user.Role == Roles.ShopOwner)
        {
            return Result<string>.Failure(new Error("InvalidOperation", "Cannot delete a shop owner."));
        }

        var isInSaleOrder = await _saleOrderRepository.ExistsByEmployeeId(user.Id);
        var isInProductsOrder = await _productsOrdersRepository.ExistsByEmployeeId(user.Id);
        Console.WriteLine($"isInSaleOrder: {isInSaleOrder}, isInProductsOrder: {isInProductsOrder}");

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