namespace Capstone.Contracts.Authentication;

public record AuthenticationResponse(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Role,
    string PhoneNumber,
    string Gender,
    DateOnly DateOfBirth,
    string ImageURL,
    DateTime CreatedAt,
    bool HasChangedPassword,
    string AccessToken
);

public record AuthenticationMobileResponse(
    string AccessToken,
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Role,
    string PhoneNumber,
    string Gender,
    DateOnly DateOfBirth,
    string ImageURL,
    DateTime CreatedAt,
    bool HasChangedPassword
);