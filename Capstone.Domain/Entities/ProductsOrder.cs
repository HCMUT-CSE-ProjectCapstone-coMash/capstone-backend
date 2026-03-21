namespace Capstone.Domain.Entities;

public class ProductsOrder
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public string OrderDescription { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;

    public ICollection<ProductsOrdersDetail> ProductsOrdersDetails { get; set; } = new List<ProductsOrdersDetail>();
}