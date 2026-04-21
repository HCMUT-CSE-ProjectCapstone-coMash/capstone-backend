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
        string promotionName,
        string promotionType,
        string description,
        string startDate,
        string endDate,
        string createdBy
    )
    {
        var promotionIdResult = await CreatePromotionId();

        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionIdResult.Value,
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

    public async Task<Result<PaginatedResult<PromotionDto>>> FetchPromotions(
        int currentPage,
        int pageSize,
        string? category = null,
        string? search = null
    )
    {
        var (promotions, total) = await _promotionsRepository.FetchPromotions(currentPage, pageSize, category, search);

        var promotionDtos = promotions.Select(p => new PromotionDto
        {
            Id = p.Id,
            PromotionId = p.PromotionId,
            PromotionName = p.PromotionName,
            PromotionType = p.PromotionType,
            Description = p.Description,
            PromotionStatus = p.PromotionStatus,
            PromotionPhase = GetPromotionPhase(p.StartDate, p.EndDate).Value,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Result<PaginatedResult<PromotionDto>>.Success(
            new PaginatedResult<PromotionDto>(promotionDtos, total));
    }

    public async Task<Result<PromotionDto>> GetPromotionById(string id)
    {
        var promotion = await _promotionsRepository.GetPromotionById(Guid.Parse(id));

        if (promotion == null)
        {
            return Result<PromotionDto>.Failure(new Error("PromotionNotFound", "Promotion not found"));
        }

        return Result<PromotionDto>.Success(new PromotionDto
        {
            Id = promotion.Id,
            PromotionId = promotion.PromotionId,
            PromotionName = promotion.PromotionName,
            PromotionType = promotion.PromotionType,
            Description = promotion.Description,
            PromotionStatus = promotion.PromotionStatus,
            PromotionPhase = GetPromotionPhase(promotion.StartDate, promotion.EndDate).Value,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            CreatedAt = promotion.CreatedAt
        });
    }

    private Result<string> GetPromotionPhase(DateOnly startDate, DateOnly endDate)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        string phase;

        if (today < startDate)
        {
            phase = PromotionPhase.Upcoming;
        }
        else if (today > endDate)
        {
            phase = PromotionPhase.Expired;
        }
        else
        {
            phase = PromotionPhase.Ongoing;
        }

        return Result<string>.Success(phase);
    }
}