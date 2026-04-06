namespace Capstone.Contracts.Products;

public record OwnerPatchInProductsOrderRequest(
    string? ProductName,
    string? Color,
    string? Pattern,
    string? SizeType,
    List<ProductQuantity>? Quantities,
    decimal? SalePrice,
    decimal? ImportPrice
);