namespace Capstone.Domain.Entities;

public class Promotion
{
    public Guid Id { get; set; }
    public string PromotionId { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public string PromotionStatus { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}