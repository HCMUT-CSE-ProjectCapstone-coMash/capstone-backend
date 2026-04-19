using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.Promotions;

public class PromotionsService : IPromotionsService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly IPromotionsRepository _promotionsRepository;

    public PromotionsService(
        IDateTimeProvider dateTimeProvider,
        IPromotionsRepository promotionsRepository
    )
    {
        _dateTimeProvider = dateTimeProvider;
        _promotionsRepository = promotionsRepository;
    }

    public async Task<Result<string>> CreatePromotionId()
    {
        var prefix = "KM";

        var maxNumber = await _promotionsRepository.GetMaxPromotionId(prefix);
        var newId = $"{prefix}-{maxNumber + 1:D5}";

        return Result<string>.Success(newId);
    }
    
    public async Task<Result<string>> CreatePromotion(
        string promotionId,
        string promotionName,
        string promotionType,
        string description,
        string startDate,
        string endDate,
        string createdBy
    )
    {
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            PromotionName = promotionName,
            PromotionType = promotionType,
            Description = description,
            StartDate = DateOnly.Parse(startDate),
            EndDate = DateOnly.Parse(endDate),
            PromotionStatus = PromotionStatus.Active.ToString(),
            CreatedBy = Guid.Parse(createdBy),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _promotionsRepository.CreatePromotion(promotion);

        return Result<string>.Success(promotion.Id.ToString());
    }
}