namespace Capstone.Contracts.Products;

public record PatchProductRequest(
    string? ProductID,
    string? ProductName,
    string? Category,
    string? Color,
    string? Pattern,
    string? SizeType,
    List<ProductQuantity>? Quantities,
    string? CreatedBy
);