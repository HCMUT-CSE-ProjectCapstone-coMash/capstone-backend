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

    public async Task<Result<List<OrderPromotionDto>>> GetOrderPromotionsByPromotionId(Guid promotionId)
    {
        var orderPromotionList = await _orderPromotionsRepository.GetOrderPromotionsByPromotionId(promotionId);

        var orderPromotionDtos = orderPromotionList.Select(op => new OrderPromotionDto
        {
            Id = op.Id,
            MinValue = op.MinValue,
            DiscountType = op.DiscountType,
            DiscountValue = op.DiscountValue,
            MaxDiscount = op.MaxDiscount
        }).ToList();

        return Result<List<OrderPromotionDto>>.Success(orderPromotionDtos);
    }

    public async Task<Result> DeleteOrderPromotionsByPromotionId(string promotionId)
    {
        await _orderPromotionsRepository.DeleteOrderPromotionsByPromotionId(Guid.Parse(promotionId));

        return Result.Success();
    }
}