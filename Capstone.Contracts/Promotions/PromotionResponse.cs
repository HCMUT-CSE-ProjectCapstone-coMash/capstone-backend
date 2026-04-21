namespace Capstone.Contracts.Promotions;

public record PromotionResponse (
    Guid Id,
    string PromotionId,
    string PromotionName,
    string PromotionType,
    string Description,
    string PromotionStatus,
    string PromotionPhase,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTime CreatedAt
);