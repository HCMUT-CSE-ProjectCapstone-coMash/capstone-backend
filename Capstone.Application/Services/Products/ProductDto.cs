namespace Capstone.Application.Services.Products;

public record ProductDto(
    Guid Id,
    string ProductId,
    string ProductName,
    string Category,
    string Color,
    string Pattern,
    string SizeType,
    List<ProductQuantityDto> Quantities,
    Guid CreatedBy,
    DateTime CreatedAt,
    string Status,
    string ImageURL,
    string VectorId
);

public record ProductQuantityDto(
    string Size,
    int Quantities
);