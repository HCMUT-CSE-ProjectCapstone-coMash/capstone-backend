namespace Capstone.Contracts.Authentication;

public record GetEmployeeResponse(
    List<EmployeeResponse> Items,
    int Total
);
