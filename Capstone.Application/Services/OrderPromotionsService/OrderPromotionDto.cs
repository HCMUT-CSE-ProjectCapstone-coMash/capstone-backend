namespace Capstone.Application.Services.OrderPromotionsService;

public class OrderPromotionDto
{
    public Guid Id { get; set; }
    public decimal MinValue { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MaxDiscount { get; set; }
}