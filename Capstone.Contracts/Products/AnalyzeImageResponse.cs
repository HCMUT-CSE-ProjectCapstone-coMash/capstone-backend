namespace Capstone.Contracts.Products;

public record AnalyzeImageResponse(
    string ProductId,
    string ProductName,
    string Category,
    string Color,
    string Pattern
);