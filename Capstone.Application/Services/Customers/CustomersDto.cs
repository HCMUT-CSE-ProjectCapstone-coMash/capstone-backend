namespace Capstone.Application.Services.Customers;

public record CustomerDto (
    Guid Id,
    string CustomerName,
    string CustomerPhone,
    string CustomerStatus,
    DateTime CreatedAt,
    Guid CreatedBy
);