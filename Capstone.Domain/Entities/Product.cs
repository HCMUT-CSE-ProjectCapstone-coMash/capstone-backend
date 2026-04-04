namespace Capstone.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string SizeType { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ImageKey { get; set; } = string.Empty;
    public string VectorId { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal ImportPrice { get; set; }

    public ICollection<ProductQuantity> ProductQuantities { get; set; } = new List<ProductQuantity>();
}