namespace Capstone.Domain.Entities;

public class ProductPromotion
{
    public Guid Id { get; set; }
    public Guid PromotionId { get; set; }
    public Guid ProductId { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public double DiscountValue { get; set; }

    public Promotion Promotion { get; set; } = null!;
    public Product Product { get; set; } = null!;
}