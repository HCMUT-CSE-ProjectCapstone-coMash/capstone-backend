namespace Capstone.Domain.Entities;

public class ComboPromotionDetail
{
    public Guid Id { get; set; }
    public Guid ComboPromotionId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    public Product Product { get; set; } = null!;
    public ComboPromotion ComboPromotion { get; set; } = null!;
}