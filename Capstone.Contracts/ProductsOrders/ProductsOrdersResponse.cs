using Capstone.Contracts.Products;

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