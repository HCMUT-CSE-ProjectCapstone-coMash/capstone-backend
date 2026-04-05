namespace Capstone.Contracts.Products;

public record OwnerPatchProductRequest(
    string? ProductId,
    string? ProductName,
    string? Category,
    string? Color,
    string? Pattern,
    string? SizeType,
    List<ProductQuantity>? Quantities,
    decimal? SalePrice,
    decimal? ImportPrice
);