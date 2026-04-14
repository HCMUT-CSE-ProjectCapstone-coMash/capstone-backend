namespace Capstone.Contracts.SaleOrders;

public record CreateCustomersRequest(
    string CustomerName,
    string customerPhone
);