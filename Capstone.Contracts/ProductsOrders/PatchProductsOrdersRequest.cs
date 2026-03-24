namespace Capstone.Contracts.ProductsOrders;

public record PatchProductsOrders(
    string? OrderName,
    string? OrderDescription,
    string? OrderStatus
);