using Capstone.Application.Common;

namespace Capstone.Application.Services.Promotions;

public interface IPromotionsService
{
    Task<Result<string>> CreatePromotionId();
    
    Task<Result<string>> CreatePromotion(
        string promotionId,
        string promotionName,
        string promotionType,
        string description,
        string startDate,
        string endDate,
        string createdBy
    );
}