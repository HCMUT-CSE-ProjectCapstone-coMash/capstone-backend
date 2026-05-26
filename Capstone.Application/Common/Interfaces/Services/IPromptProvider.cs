namespace Capstone.Application.Common.Interfaces.Services;

public interface IPromptProvider
{
    Task<AnalyzeProduct> AnalyzeImageWithClaude(string ImageBase64, List<string> categories, List<string> colors, List<string> patterns);
}

public record AnalyzeProduct(
    string Category,
    string Color,
    string Pattern
);