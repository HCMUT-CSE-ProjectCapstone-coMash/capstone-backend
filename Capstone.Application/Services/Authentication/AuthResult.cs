namespace Capstone.Application.Services.Authentication;

public record AuthResult(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAt,
    string Token,
    string PhoneNumber,
    string Gender,
    DateOnly DateOfBirth,
    string ImageURL
);

public record UserDto(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAt,
    string PhoneNumber,
    string Gender,
    DateOnly DateOfBirth,
    string ImageURL
);
