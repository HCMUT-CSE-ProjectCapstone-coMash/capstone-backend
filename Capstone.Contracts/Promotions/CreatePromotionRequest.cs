using System.Text.Json.Serialization;

namespace Capstone.Contracts.Promotions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "promotionType")]
[JsonDerivedType(typeof(CreateProductPromotionRequest), "Product")]
[JsonDerivedType(typeof(CreateComboPromotionRequest),   "Combo")]
[JsonDerivedType(typeof(CreateOrderPromotionRequest),   "Order")]
public abstract class CreatePromotionRequest
{
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
    public double DiscountValue { get; set; }
}

public class ComboDealDto
{
    public string ComboName { get; set; } = string.Empty;
    public List<ComboItemDto> ComboItems { get; set; } = new();
    public double ComboPrice { get; set; }
}

public class ComboItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class PromotionLevelDto
{
    public double MinValue { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public double DiscountValue { get; set; }
    public double? MaxDiscount { get; set; }
}