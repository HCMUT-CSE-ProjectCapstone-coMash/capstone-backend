using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductPromotionsService;

public class ProductPromotionsService : IProductPromotionsService
{
    public Task<Result> CreateProductPromotion(string promotionId, string productId, string discountType, decimal discountValue)
    {
        throw new NotImplementedException();
    }
}