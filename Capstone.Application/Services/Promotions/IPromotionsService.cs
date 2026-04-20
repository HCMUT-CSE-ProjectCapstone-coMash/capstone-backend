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
}