using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.ComboPromotionsService;

public class ComboPromotionDto
{
    public Guid Id { get; set; }
    public string ComboName { get; set; } = string.Empty;
    public double ComboPrice { get; set; }
    public List<ComboPromotionDetailDto> ComboItems { get; set; } = new();
    public Guid PromotionId { get; set; }
}

public class ComboPromotionDetailDto
{
    public Guid Id { get; set; }
    public ProductDto Product { get; set; } = null!;
    public int Quantity { get; set; }
}