using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductPromotionsService;

public interface IProductPromotionsService
{
    Task<Result> CreateProductPromotion(string promotionId, string productId, string discountType, double discountValue);

    Task<Result<List<ProductPromotionDto>>> GetProductPromotionsByPromotionId(Guid promotionId);

    Task<Result> DeleteProductPromotionsByPromotionId(Guid promotionId);
}