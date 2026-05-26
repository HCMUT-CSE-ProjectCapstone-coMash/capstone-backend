namespace Capstone.Contracts.Products;

public record OwnerPatchInProductsOrderRequest(
    string? ProductName,
    string? ColorId,
    string? PatternId,
    string? SizeType,
    List<ProductQuantity>? Quantities,
    double? SalePrice,
    double? ImportPrice
);