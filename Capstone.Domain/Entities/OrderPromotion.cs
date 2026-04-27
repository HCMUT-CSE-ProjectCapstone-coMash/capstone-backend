namespace Capstone.Domain.Entities
{
    public class OrderPromotion
    {
        public Guid Id { get; set; }
        public Guid PromotionId { get; set; }
        public double MinValue { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public double DiscountValue { get; set; }
        public double MaxDiscount { get; set; }

        public Promotion Promotion { get; set; } = null!;
    }
}