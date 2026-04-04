namespace Capstone.Domain.Entities;

public class ProductsOrdersDetail
{
    public Guid Id { get; set; }
    public Guid ProductsOrderId { get; set; }
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
    public ICollection<ProductsOrdersDetailQuantityChange> QuantityChanges { get; set; } = new List<ProductsOrdersDetailQuantityChange>();
}