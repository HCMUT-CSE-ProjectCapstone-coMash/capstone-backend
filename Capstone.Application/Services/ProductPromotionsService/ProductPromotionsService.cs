using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.ProductPromotionsService;

public class ProductPromotionsService : IProductPromotionsService
{
    private readonly IProductPromotionsRepository _productPromotionsRepository;

    public ProductPromotionsService(IProductPromotionsRepository productPromotionsRepository)
    {
        _productPromotionsRepository = productPromotionsRepository;
    }

    public async Task<Result> CreateProductPromotion(string promotionId, string productId, string discountType, decimal discountValue)
    {
        var productPromotion = new ProductPromotion
        {
            Id = Guid.NewGuid(),
            PromotionId = Guid.Parse(promotionId),
            ProductId = Guid.Parse(productId),
            DiscountType = discountType,
            DiscountValue = discountValue
        };

        await _productPromotionsRepository.CreateProductPromotion(productPromotion);

        return Result.Success();
    }
}