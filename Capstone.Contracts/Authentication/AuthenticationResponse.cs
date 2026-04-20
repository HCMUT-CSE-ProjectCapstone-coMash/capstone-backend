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
    DateTime CreatedAt
);