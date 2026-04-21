using Capstone.Application.Common;

namespace Capstone.Application.Services.OrderPromotionsService;

public interface IOrderPromotionsService
{
    Task<Result> CreateOrderPromotion(
        string promotionId,
        decimal minValue,
        string discountType,
        decimal discountValue,
        decimal maxDiscount
    );

    Task<Result<List<OrderPromotionDto>>> GetOrderPromotionsByPromotionId(Guid promotionId);
}