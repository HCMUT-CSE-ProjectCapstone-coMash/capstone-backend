namespace Capstone.Domain.Entities;

public class SaleOrderDetail
{
    public Guid Id { get; set; }
    public Guid SaleOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string SelectedSize { get; set; } = null!;
    public double UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double Discount { get; set; }
    public double SubTotal { get; set; }
    public double Profit { get; set; }
    public Guid? ProductPromotionId { get; set; }
    public Guid? ComboPromotionId { get; set; }

    public Product Product { get; set; } = null!;
    public SaleOrder SaleOrder { get; set; } = null!;
    public ProductPromotion? ProductPromotion { get; set; }
    public ComboPromotion? ComboPromotion { get; set; }
}