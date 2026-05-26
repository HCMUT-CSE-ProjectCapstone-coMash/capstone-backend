namespace Capstone.Contracts.Products;

public record EmployeePatchInProductsOrderRequest(
    string? ProductName,
    string? ColorId,
    string? PatternId,
    string? SizeType,
    List<ProductQuantity>? Quantities
);