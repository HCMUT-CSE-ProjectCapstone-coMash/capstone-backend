namespace Capstone.Contracts.Authentication;

public record GetEmployeeByIdResponse(
    string EmployeeId,
    string FullName,
    string Email,
    string Role,
    string PhoneNumber,
    string Gender,
    string DateOfBirth,
    string ImageURL
);