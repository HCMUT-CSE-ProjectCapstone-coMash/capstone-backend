namespace Capstone.Contracts.Products;

public record ProductResponse(
    Guid Id,
    string ProductId,
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
    double SalePrice,
    double ImportPrice,
    List<ProductQuantityChange>? QuantityChanges = null
);

public record ProductQuantity(
    string Size,
    int Quantities
);

public record ProductQuantityChange(
    string Size,
    int OldQuantity,
    int NewQuantity
);

public record ProductWithOrderStatusResponse(
    Guid Id,
    string ProductId,
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
    double SalePrice,
    double ImportPrice,
    bool IsInPendingOrder,
    List<ProductQuantityChange>? QuantityChanges = null
);