using Capstone.Application.Common;

namespace Capstone.Application.Services.Promotions;

public interface IPromotionsService
{
    Task<Result<string>> CreatePromotionId();

    Task<Result<string>> CreatePromotion(
        string promotionName,
        string promotionType,
        string description,
        string startDate,
        string endDate,
        string createdBy
    );

    Task<Result<PaginatedResult<PromotionDto>>> FetchPromotions(
        int currentPage,
        int pageSize,
        string? category = null,
        string? search = null
    );

    Task<Result<PromotionDto>> GetPromotionById(string promotionId);

    Task<Result<string>> UpdatePromotion(
        string promotionId,
        string promotionName,
        string startDate,
        string endDate,
        string description
    );

    Task<Result<List<PromotionDto>>> GetProductPromotionsByProductId(string productId);

    Task<Result<List<PromotionDto>>> GetComboPromotionsByProductId(string productId);

    Task<Result<List<PromotionDto>>> GetActiveOrderPromotions();
}