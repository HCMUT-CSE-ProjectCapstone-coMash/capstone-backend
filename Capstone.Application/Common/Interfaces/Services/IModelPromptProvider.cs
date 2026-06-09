namespace Capstone.Application.Common.Interfaces.Services;

public interface IModelPromptProvider
{
    /// <summary>
    /// Generate an image of a model wearing the product using Gemini AI
    /// </summary>
    /// <param name="productImageBase64">Product image in base64 format</param>
    /// <param name="productName">Name of the product</param>
    /// <returns>Generated image in base64 format</returns>
    Task<string> GenerateModelImageAsync(string productImageBase64, string productName);
}
