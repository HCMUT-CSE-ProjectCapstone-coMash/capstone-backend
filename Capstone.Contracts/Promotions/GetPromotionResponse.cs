namespace Capstone.Contracts.Promotions;

public record GetPromotionResponse
(
    List<PromotionResponse> Items,
    int Total
);