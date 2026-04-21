using Capstone.Application.Services.ComboPromotionsService;
using Capstone.Application.Services.OrderPromotionsService;
using Capstone.Application.Services.ProductPromotionsService;

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

    public List<ProductPromotionDto>? ProductPromotions { get; set; }
    public List<OrderPromotionDto>? OrderPromotions { get; set; }
    public List<ComboPromotionDto>? ComboPromotions { get; set; }
};