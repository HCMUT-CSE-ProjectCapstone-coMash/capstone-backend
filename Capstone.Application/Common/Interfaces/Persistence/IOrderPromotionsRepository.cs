using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence
{
    public interface IOrderPromotionsRepository
    {
        Task CreateOrderPromotion(OrderPromotion orderPromotion);

        Task<List<OrderPromotion>> GetOrderPromotionsByPromotionId(Guid promotionId);

        Task DeleteOrderPromotionsByPromotionId(Guid promotionId);

        Task<OrderPromotion?> GetOrderPromotionByOrderPromotionId(Guid orderPromotionId);
    }
}