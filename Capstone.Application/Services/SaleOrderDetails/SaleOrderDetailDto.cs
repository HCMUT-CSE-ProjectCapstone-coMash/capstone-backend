using Capstone.Application.Services.ComboPromotionsService;
using Capstone.Application.Services.ProductPromotionsService;

namespace Capstone.Application.Services.SaleOrderDetails;

public class SaleOrderDetailDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SelectedSize { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double Discount { get; set; }
    public double SubTotal { get; set; }
    public double Profit { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public ProductPromotionDto? ProductPromotion { get; set; }
    public ComboPromotionDto? ComboPromotion { get; set; }
}