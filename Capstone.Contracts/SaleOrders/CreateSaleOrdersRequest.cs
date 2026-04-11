namespace Capstone.Contracts.SaleOrders;

public record SaleProductRequest(
    string ProductId,
    string SelectedSize,
    int Quantity,
    double Discount
);

public record CreateSaleOrdersRequest(
    string CustomerId,
    string UserId,
    string PaymentMethod,
    double DebitMoney,
    double Discount,
    List<SaleProductRequest> Products
);