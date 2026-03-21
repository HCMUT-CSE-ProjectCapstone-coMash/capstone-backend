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
    string VectorId
);

public record ProductQuantityResponse(
    string Size,
    int Quantities
);