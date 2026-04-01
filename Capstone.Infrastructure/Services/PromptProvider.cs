using Google.GenAI;
using Google.GenAI.Types;
using Capstone.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Capstone.Infrastructure.Services;

public class PromptProvider : IPromptProvider
{
    private readonly GeminiSettings _settings;
    private readonly Client _client;

    public PromptProvider(IOptions<GeminiSettings> settings)
    {
        _settings = settings.Value;
        _client = new Client(apiKey: _settings.APIKey);
    }

    public async Task<AnalyzeProduct> AnalyzeImage(string imageBase64, string[] categories, string[] colors, string[] patterns)
    {
        var prompt = $$"""
            Analyze this product image and return ONLY a JSON object with no markdown or preamble.
            Pick the best match for each field from the allowed values only.

            Allowed categories: {{string.Join(", ", categories)}}
            Allowed colors: {{string.Join(", ", colors)}}
            Allowed patterns: {{string.Join(", ", patterns)}}

            Return this exact shape:
            {
              "category": "<one of the allowed categories, or empty string if unsure>",
              "color": "<one of the allowed colors, or empty string if unsure>",
              "pattern": "<one of the allowed patterns, or empty string if unsure>"
            }
            """;

        var base64Data = imageBase64.Contains(',') ? imageBase64.Split(',')[1]: imageBase64;
        
        var response = await _client.Models.GenerateContentAsync(
            model: _settings.Model,
            contents: new Content {
                Parts = [
                    new Part { Text = prompt },
                    new Part {
                        InlineData = new Blob {
                            MimeType = "image/png",
                            Data = Convert.FromBase64String(base64Data)
                        }
                    }
                ]
            }
        );

        var text = response.Candidates![0].Content!.Parts![0].Text ?? "{}";

        text = text.Trim().TrimStart('`');
        if (text.StartsWith("json")) text = text[4..];
        text = text.TrimEnd('`').Trim();

        return JsonSerializer.Deserialize<AnalyzeProduct>(text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new AnalyzeProduct("", "", "");
    }
}