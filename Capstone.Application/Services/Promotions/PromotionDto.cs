namespace Capstone.Application.Services.Promotions;

public class PromotionDto
{
    public Guid Id { get; set; }
    public string PromotionId { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PromotionStatus { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}