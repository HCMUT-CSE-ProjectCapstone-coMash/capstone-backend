namespace Capstone.Domain.Entities;

public class SaleOrderDetail
{
    public Guid Id { get; set; }
    public Guid SaleOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public double Discount { get; set; }

    public Product Product { get; set; } = null!;
    public SaleOrder SaleOrder { get; set; } = null!;
}