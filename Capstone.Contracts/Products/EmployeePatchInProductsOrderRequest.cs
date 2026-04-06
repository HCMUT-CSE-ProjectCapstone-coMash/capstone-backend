namespace Capstone.Contracts.Products;

public record EmployeePatchInProductsOrderRequest(
    string? ProductName,
    string? Color,
    string? Pattern,
    string? SizeType,
    List<ProductQuantity>? Quantities
);