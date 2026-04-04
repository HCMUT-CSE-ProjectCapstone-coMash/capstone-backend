namespace Capstone.Contracts.ProductsOrders;

public record ProductsOrdersResponse(
    Guid Id,
    Guid CreatedBy,
    DateTime CreatedAt,
    string OrderName,
    string OrderDescription,
    string OrderStatus,
    List<ProductResponse> Products
);

public record ProductsOrdersListResponse(
    Guid Id,
    Guid CreatedBy,
    string CreatedByName,
    DateTime CreatedAt,
    string OrderName,
    string OrderDescription,
    string OrderStatus
);

public record ProductResponse(
    Guid Id,
    string ProductId,
    string ProductName,
    string Category,
    string Color,
    string Pattern,
    string SizeType,
    List<ProductQuantityResponse> Quantities,
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

public record ProductQuantityResponse(
    string Size,
    int Quantities
);