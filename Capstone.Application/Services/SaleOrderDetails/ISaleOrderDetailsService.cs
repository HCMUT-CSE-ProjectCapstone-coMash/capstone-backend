using Capstone.Application.Common;

namespace Capstone.Application.Services.SaleOrderDetails;

public interface ISaleOrderDetailsService
{
    Task<Result> CreateSaleOrderDetailForProductPromotion(
        string SaleOrderId,
        string ProductId,
        string SelectedSize,
        int Quantity,
        double Discount,
        string PromotionId
    );

    Task<Result> CreateSaleOrderDetailForComboPromotion(
        string SaleOrderId,
        string ProductId,
        string SelectedSize,
        int TotalProductQuantity,
        int ComboQuantity,
        string PromotionId
    );
}