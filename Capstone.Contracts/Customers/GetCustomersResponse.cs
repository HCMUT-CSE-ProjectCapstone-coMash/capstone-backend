namespace Capstone.Contracts.Customers;

public record GetCustomersResponse(
    List<CustomersResponse> Items,
    int Total
);
