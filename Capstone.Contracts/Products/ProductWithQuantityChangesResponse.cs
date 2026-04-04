namespace Capstone.Contracts.Products;

public record ProductWithQuantityChangesResponse(
    Guid Id,
    string ProductID,
    string ProductName,
    string Category,
    string Color,
    string Pattern,
    string SizeType,
    List<ProductQuantity> Quantities,
    Guid CreatedBy,
    DateTime CreatedAt,
    string Status,
    string ImageURL,
    string VectorId,
    List<ProductQuantityChangeResponse> QuantityChanges
);

public record ProductQuantityChangeResponse(
    string Size,
    int OldQuantity,
    int NewQuantity
);