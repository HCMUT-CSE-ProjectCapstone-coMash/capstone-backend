namespace Capstone.Contracts.Products;

public record CreateProductResponse(
    Guid Id,
    string ProductID,
    string ProductName,
    string Category,
    string Color,
    string Pattern,
    string SizeType,
    List<ProductQuantityDTO> Quantities
);