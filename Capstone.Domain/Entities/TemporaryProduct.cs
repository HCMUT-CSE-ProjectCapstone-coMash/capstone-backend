namespace Capstone.Domain.Entities;

public class TemporaryProduct
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string ImageKey { get; set; } = string.Empty;
}