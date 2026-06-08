using Capstone.Application.Common.Interfaces.Services;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace Capstone.Infrastructure.Services;

public class ModelPromptProvider : IModelPromptProvider
{
    private readonly GeminiSettings _geminiSettings;
    private readonly Client _client;

    public ModelPromptProvider(IOptions<GeminiSettings> geminiSettings)
    {
        _geminiSettings = geminiSettings.Value;
        _client = new Client(apiKey: _geminiSettings.APIKey);
    }

    public async Task<string> GenerateModelImageAsync(string productImageBase64, string productName)
    {
        var mimeType = productImageBase64.StartsWith("data:image/png") ? "image/png" : "image/jpeg";
        var base64Data = productImageBase64.Contains(',')
            ? productImageBase64.Split(',')[1]
            : productImageBase64;

        var promptText = $"""
            Generate a photorealistic image of an Asian middle-aged woman (40-55 years old) 
            with black hair wearing the product shown in the reference image.
            Requirements:
            - White background
            - Front-facing view, full outfit shown
            - Professional photography style
            - Product: {productName}
            Generate the image based on the reference product image provided.
            """;

        var contents = new List<Content>
        {
            new Content
            {
                Role = "user",
                Parts = new List<Part>
                {
                    new Part { Text = promptText },
                    new Part
                    {
                        InlineData = new Blob
                        {
                            MimeType = mimeType,
                            Data = Convert.FromBase64String(base64Data)
                        }
                    }
                }
            }
        };

        var config = new GenerateContentConfig
        {
            ResponseModalities = new List<string> { "TEXT", "IMAGE" },
            HttpOptions = new HttpOptions
            {
                Timeout = 300000
            }
        };

        try
        {
            var response = await _client.Models.GenerateContentAsync(
                model: _geminiSettings.Model,  
                contents: contents,
                config: config
            );

            var parts = response?.Candidates?.FirstOrDefault()?.Content?.Parts;
            if (parts != null)
            {
                var imagePart = parts.FirstOrDefault(p => p.InlineData?.Data != null);
                if (imagePart?.InlineData != null)
                {
                    var outputMime = imagePart.InlineData.MimeType ?? "image/jpeg";
                    var base64Result = Convert.ToBase64String(imagePart.InlineData.Data);
                    return $"data:{outputMime};base64,{base64Result}";
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Gemini GenAI image production failed: {ex.Message}", ex);
        }

        throw new InvalidOperationException("No image was returned by the Gemini model.");
    }
    public async Task WarmupAsync()
    {
        await _client.Models.GenerateContentAsync(
            model: _geminiSettings.Model,
            contents: "hi",
            config: new GenerateContentConfig
            {
                HttpOptions = new HttpOptions { Timeout = 30000 }
            }
        );
    }
}