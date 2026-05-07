namespace Capstone.Application.Common.Interfaces.Services;

public interface IPromptProvider
{
    Task<AnalyzeProduct> AnalyzeImageWithClaude(string ImageBase64);
}

public record AnalyzeProduct(
    string Category,
    string Color,
    string Pattern
);