namespace Capstone.Contracts.Products;

public record OwnerPatchProductRequest(
    string? ProductId,
    string? ProductName,
    string? CategoryId,
    string? ColorId,
    string? PatternId,
    string? SizeType,
    List<ProductQuantity>? Quantities,
    double? SalePrice,
    double? ImportPrice
);