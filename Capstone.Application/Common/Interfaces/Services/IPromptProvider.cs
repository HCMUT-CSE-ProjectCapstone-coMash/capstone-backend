namespace Capstone.Application.Common.Interfaces.Services;

public interface IPromptProvider
{
    Task<AnalyzeProduct> AnalyzeImage(string ImageBase64, string[] Categories, string[] Colors, string[] Patterns);
}

public record AnalyzeProduct(
    string Category,
    string Color,
    string Pattern
);