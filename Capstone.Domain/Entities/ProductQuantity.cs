namespace Capstone.Domain.Entities;

public class ProductQuantity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Size { get; set; } = string.Empty;
    public int Quantities { get; set; }

    public Product Product { get; set; } = null!;
}