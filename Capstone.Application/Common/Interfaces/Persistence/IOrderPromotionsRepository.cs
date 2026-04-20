using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence
{
    public interface IOrderPromotionsRepository
    {
        Task CreateOrderPromotion(OrderPromotion orderPromotion);
    }
}