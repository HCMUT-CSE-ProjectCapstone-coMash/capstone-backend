using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.ProductPromotionsService;

public class ProductPromotionDto
{
    public Guid Id { get; set; }
    public ProductDto Product { get; set; } = null!;
    public string DiscountType { get; set; } = string.Empty;
    public double DiscountValue { get; set; }
    public Guid PromotionId { get; set; }
    public string PromotionName { get; set; } = string.Empty;
}