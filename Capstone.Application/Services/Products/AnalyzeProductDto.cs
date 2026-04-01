namespace Capstone.Application.Services.Products;

public record AnalyzeProductDto(
    string ProductId,
    string ProductName,
    string Category,
    string Color,
    string Pattern
);