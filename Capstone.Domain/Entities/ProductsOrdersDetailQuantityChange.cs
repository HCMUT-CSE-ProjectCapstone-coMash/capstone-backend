namespace Capstone.Domain.Entities;

public class ProductsOrdersDetailQuantityChange
{
    public Guid Id { get; set; }
    public Guid ProductsOrdersDetailId { get; set; }
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public string Size { get; set; } = string.Empty;

    public ProductsOrdersDetail ProductsOrdersDetail { get; set; } = null!;
}