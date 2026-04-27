namespace Capstone.Application.Services.OrderPromotionsService;

public class OrderPromotionDto
{
    public Guid Id { get; set; }
    public double MinValue { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public double DiscountValue { get; set; }
    public double MaxDiscount { get; set; }
}