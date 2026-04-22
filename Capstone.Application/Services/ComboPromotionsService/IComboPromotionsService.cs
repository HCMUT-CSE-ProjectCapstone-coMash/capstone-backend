using Capstone.Application.Common;

namespace Capstone.Application.Services.ComboPromotionsService;

public interface IComboPromotionsService
{
    Task<Result<string>> CreateComboPromotion(string promotionId, string comboName, decimal comboPrice);

    Task<Result> CreateComboPromotionDetail(string comboPromotionId, string productId, int quantity);

    Task<Result<List<ComboPromotionDto>>> GetComboPromotionsByPromotionId(Guid promotionId);
    
    Task<Result> DeleteComboPromotionsByPromotionId(string promotionId);
}