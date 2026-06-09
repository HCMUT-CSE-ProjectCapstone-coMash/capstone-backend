namespace Capstone.Domain.Entities;

public class TemporaryProduct
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid ColorId { get; set; }
    public Guid PatternId { get; set; }
    public string ImageKey { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Color Color { get; set; } = null!;
    public Pattern Pattern { get; set; } = null!;
}