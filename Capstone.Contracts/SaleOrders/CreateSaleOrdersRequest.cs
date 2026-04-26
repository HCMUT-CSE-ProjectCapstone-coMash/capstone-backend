namespace Capstone.Contracts.SaleOrders;

public record SaleProductRequest(
    string ProductId,
    string SelectedSize,
    int Quantity,
    double Discount,
    string PromotionId
);

public record SaleComboItemRequest(
    string ProductId,
    string SelectedSize,
    int Quantity
);

public record SaleComboRequest(
    string ComboDealId,
    int Quantity,
    List<SaleComboItemRequest> Items
);

public record CreateSaleOrdersRequest(
    string CustomerId,
    string UserId,
    string PaymentMethod,
    double DebtAmount,
    List<SaleProductRequest> Products,
    List<SaleComboRequest> Combos
);