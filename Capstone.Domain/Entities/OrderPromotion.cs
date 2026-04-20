namespace Capstone.Domain.Entities
{
    public class OrderPromotion
    {
        public Guid Id { get; set; }
        public Guid PromotionId { get; set; }
        public decimal MinValue { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MaxDiscount { get; set; }

        public Promotion Promotion { get; set; } = null!;
    }
}