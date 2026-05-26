using Anthropic;
using Anthropic.Models.Messages;
using Capstone.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Capstone.Infrastructure.Services;

public class PromptProvider : IPromptProvider
{
    private readonly ClaudeSettings _claudeSettings;
    private readonly AnthropicClient _client;

    public PromptProvider(IOptions<ClaudeSettings> claudeSettings)
    {
        _claudeSettings = claudeSettings.Value;
        _client = new AnthropicClient(new Anthropic.Core.ClientOptions { ApiKey = _claudeSettings.ApiKey });
    }

    public async Task<AnalyzeProduct> AnalyzeImageWithClaude(string ImageBase64, List<string> categories, List<string> colors, List<string> patterns)
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

        var base64Data = ImageBase64.Contains(',') ? ImageBase64.Split(',')[1] : ImageBase64;

        // Detect media type
        MediaType mediaType = MediaType.ImageJpeg;
        if (ImageBase64.Contains("data:"))
        {
            var mimeType = ImageBase64.Split(';')[0].Split(':')[1];
            mediaType = mimeType switch
            {
                "image/png"  => MediaType.ImagePng,
                "image/gif"  => MediaType.ImageGif,
                "image/webp" => MediaType.ImageWebP,
                _            => MediaType.ImageJpeg
            };
        }
        else
        {
            var bytes = Convert.FromBase64String(base64Data[..Math.Min(16, base64Data.Length)]);
            if (bytes[0] == 0x89 && bytes[1] == 0x50) mediaType = MediaType.ImagePng;
            else if (bytes[0] == 0x47 && bytes[1] == 0x49) mediaType = MediaType.ImageGif;
            else if (bytes[0] == 0x52 && bytes[1] == 0x49) mediaType = MediaType.ImageWebP;
        }

        var parameters = new MessageCreateParams
        {
            Model = _claudeSettings.Model,
            MaxTokens = 256,
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = new MessageParamContent(new List<ContentBlockParam>
                    {
                        new ContentBlockParam(new ImageBlockParam(
                            new ImageBlockParamSource(new Base64ImageSource
                            {
                                Data = base64Data,
                                MediaType = mediaType,
                            })
                        )),
                        new ContentBlockParam(new TextBlockParam(prompt)),
                    })
                }
            ]
        };

        var response = await _client.Messages.Create(parameters);

        var text = response.Content
            .Select(b => b.Value)
            .OfType<TextBlock>()
            .FirstOrDefault()?.Text ?? "{}";

        text = text.Trim().TrimStart('`');
        if (text.StartsWith("json")) text = text[4..];
        text = text.TrimEnd('`').Trim();

        return JsonSerializer.Deserialize<AnalyzeProduct>(text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new AnalyzeProduct("", "", "");
    }
}