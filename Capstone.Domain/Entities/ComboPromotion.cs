namespace Capstone.Domain.Entities;

public class ComboPromotion
{
    public Guid Id { get; set; }
    public Guid PromotionId { get; set; }
    public string ComboName { get; set; } = string.Empty;
    public decimal ComboPrice { get; set; }

    public Promotion Promotion { get; set; } = null!;
    public ICollection<ComboPromotionDetail> ComboPromotionDetails { get; set; } = new List<ComboPromotionDetail>();
}