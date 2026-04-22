namespace Capstone.Contracts.Promotions;

public abstract class UpdatePromotionRequest
{
    public string PromotionName { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProductPromotionRequest : UpdatePromotionRequest
{
    public List<ProductDiscountItemDto> ProductDiscounts { get; set; } = new();
}

public class UpdateComboPromotionRequest : UpdatePromotionRequest
{
    public List<ComboDealDto> Combos { get; set; } = new();
}

public class UpdateOrderPromotionRequest : UpdatePromotionRequest
{
    public List<PromotionLevelDto> Levels { get; set; } = new();
}