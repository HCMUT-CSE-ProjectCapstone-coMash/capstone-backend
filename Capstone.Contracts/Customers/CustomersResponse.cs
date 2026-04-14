namespace Capstone.Contracts.Customers;

public record CustomersResponse(
    Guid Id,
    string CustomerName,
    string CustomerPhone,
    string CustomerStatus,
    DateTime CreatedAt,
    Guid CreatedBy
);
