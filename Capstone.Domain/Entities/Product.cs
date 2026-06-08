namespace Capstone.Domain.Entities;

using Pgvector;

public class Product
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid ColorId { get; set; }
    public Color Color { get; set; } = null!;

    public Guid PatternId { get; set; }
    public Pattern Pattern { get; set; } = null!;

    public string SizeType { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ImageKey { get; set; } = string.Empty;
    public string VectorId { get; set; } = string.Empty;
    public Vector? Embedding { get; set; } 
    public double SalePrice { get; set; }
    public double ImportPrice { get; set; }
    public string ModelImageKey { get; set; } = string.Empty;

    public ICollection<ProductQuantity> ProductQuantities { get; set; } = new List<ProductQuantity>();
}