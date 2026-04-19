using System.Text.Json.Serialization;

namespace Capstone.Contracts.Promotions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "promotionType")]
[JsonDerivedType(typeof(CreateProductPromotionRequest), "PRODUCT")]
[JsonDerivedType(typeof(CreateComboPromotionRequest),   "COMBO")]
[JsonDerivedType(typeof(CreateOrderPromotionRequest),   "ORDER")]
public abstract class CreatePromotionRequest
{
    public string PromotionId { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateProductPromotionRequest : CreatePromotionRequest
{
    public List<ProductDiscountItemDto> ProductDiscounts { get; set; } = new();
}

public class CreateComboPromotionRequest : CreatePromotionRequest
{
    public List<ComboDealDto> Combos { get; set; } = new();
}

public class CreateOrderPromotionRequest : CreatePromotionRequest
{
    public List<PromotionLevelDto> Levels { get; set; } = new();
}

public class ProductDiscountItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty; // "PERCENT" | "FIXED"
    public decimal DiscountValue { get; set; }
}

public class ComboDealDto
{
    public string? ComboId  { get; set; }
    public string? Name     { get; set; }
    public List<ComboItemDto> Items { get; set; } = new();
    public decimal ComboPrice { get; set; }
}

public class ComboItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class PromotionLevelDto
{
    public decimal MinValue { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscount { get; set; }
}