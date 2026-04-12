namespace Capstone.Contracts.SaleOrders;

public record SaleOrderResponse(
    Guid Id,
    Guid? CustomerId,
    string? CustomerName,
    Guid CreatedBy,
    string CreatedByName,
    string PaymentMethod,
    double DebitMoney,
    DateTime CreatedAt,
    double TotalPrice,
    List<SaleOrderDetailResponse> Details
);

public record SaleOrderDetailResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string SelectedSize,
    int Quantity,
    double UnitPrice,
    double Discount,
    double SubTotal
);