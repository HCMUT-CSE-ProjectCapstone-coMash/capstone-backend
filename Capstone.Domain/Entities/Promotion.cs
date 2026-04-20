namespace Capstone.Domain.Entities;

public class Promotion
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public string PromotionId { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public string PromotionStatus { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
    public ICollection<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();
}