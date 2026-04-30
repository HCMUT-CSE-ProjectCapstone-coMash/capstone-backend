using Capstone.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Capstone.Infrastructure.Services;

public class VectorStoreProvider : IVectorStoreProvider
{
    private readonly HttpClient _httpClient;
    private readonly VectorStoreSettings _settings;

    public VectorStoreProvider(HttpClient httpClient, IOptions<VectorStoreSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }
    
    public async Task<string> InsertImageAsync(string imageUrl, object metadata)
    {
        var payload = new
        {
            image_url = imageUrl,
            metadata = metadata
        };

        var response = await _httpClient.PostAsJsonAsync(
            _settings.DatabaseURL + $"/databases/{_settings.DatabaseID}/documents",
            payload
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<InsertResponse>();
        return result?.VectorId ?? string.Empty;
    }
}

public record InsertResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("vector_id")] string VectorId
);