namespace Capstone.Contracts.Authentication;

public record EditEmployeeRequest(
    string? FullName,
    string? Gender,
    string? DateOfBirth,
    string? PhoneNumber,
    string? Email
);