namespace Capstone.Application.Services.Products;

public record CreateProductResult(
    Guid Id,
    string ProductID,
    string ProductName,
    string Category,
    string Color,
    string Pattern,
    string SizeType,
    List<ProductQuantity> Quantities
);