namespace Capstone.Application.Services.Products;

public record ProductWithQuantityChangesDto(
    ProductDto Product,
    List<ProductQuantityChangeDto> QuantityChanges
);

public record ProductQuantityChangeDto(
    string Size,
    int OldQuantity,
    int NewQuantity
);