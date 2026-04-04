using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.ProductsOrders;

public record ProductsOrdersDto(
    Guid Id,
    Guid CreatedBy,
    DateTime CreatedAt,
    string OrderName,
    string OrderDescription,
    string OrderStatus,
    List<ProductWithQuantityChangesDto> Products
);

public record ProductWithQuantityChangesDto(
    ProductDto Product,
    List<ProductQuantityChangeDto> QuantityChanges
);

public record ProductQuantityChangeDto(
    string Size,
    int OldQuantity,
    int NewQuantity
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