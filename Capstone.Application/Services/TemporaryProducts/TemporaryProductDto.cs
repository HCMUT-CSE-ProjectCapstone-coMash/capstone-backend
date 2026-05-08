namespace Capstone.Application.Services.TemporaryProducts;

public class TemporaryProductDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
}