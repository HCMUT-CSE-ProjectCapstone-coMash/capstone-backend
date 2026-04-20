using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.OrderPromotionsService;

public class OrderPromotionsService : IOrderPromotionsService
{
    private readonly IOrderPromotionsRepository _orderPromotionsRepository;

    public OrderPromotionsService(IOrderPromotionsRepository orderPromotionsRepository)
    {
        _orderPromotionsRepository = orderPromotionsRepository;
    }

    public async Task<Result> CreateOrderPromotion(string promotionId, decimal minValue, string discountType, decimal discountValue, decimal maxDiscount)
    {
        var newOrderPromotion = new OrderPromotion
        {
            Id = Guid.NewGuid(),
            PromotionId = Guid.Parse(promotionId),
            MinValue = minValue,
            DiscountType = discountType,
            DiscountValue = discountValue,
            MaxDiscount = maxDiscount
        };

        await _orderPromotionsRepository.CreateOrderPromotion(newOrderPromotion);

        return Result.Success();
    }
}