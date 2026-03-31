using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.ProductsOrders;

public record ProductsOrdersDto(
    Guid Id,
    Guid CreatedBy,
    DateTime CreatedAt,
    string OrderName,
    string OrderDescription,
    string OrderStatus,
    List<ProductDto> Products
);

public record ProductsOrdersListDto(
    Guid Id,
    Guid CreatedBy,
    string CreatedByName,
    DateTime CreatedAt,
    string OrderName,
    string OrderDescription,
    string OrderStatus
);