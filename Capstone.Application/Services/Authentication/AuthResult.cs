namespace Capstone.Application.Services.Authentication;

public record AuthResult(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAt,
    string Token
);

public record RegisterDto(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAt,
    string Token,
    string PhoneNumber,
    string Gender,
    string DateOfBirth,
    string ImageURL
);

public record EmployeeDto(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAt,
    string PhoneNumber,
    string Gender,
    string DateOfBirth,
    string ImageURL
);
public record UserDto(
    string Id,
    string FullName,
    string Email,
    string Role,
    string PhoneNumber,
    string Gender,
    string DateOfBirth
);