using Capstone.Application.Common;
using Capstone.Application.Services.Promotions;

namespace Capstone.Application.Services.OrderPromotionsService;

public interface IOrderPromotionsService
{
    Task<Result> CreateOrderPromotion(
        string promotionId,
        double minValue,
        string discountType,
        double discountValue,
        double maxDiscount
    );

    Task<Result<List<OrderPromotionDto>>> GetOrderPromotionsByPromotionId(Guid promotionId);

    Task<Result> DeleteOrderPromotionsByPromotionId(string promotionId);
}