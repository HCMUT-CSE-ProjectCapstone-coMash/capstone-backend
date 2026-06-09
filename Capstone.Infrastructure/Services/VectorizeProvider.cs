using System.Net.Http.Json;
using Capstone.Application.Common.Interfaces.Services;

namespace Capstone.Infrastructure.Services;

public class VectorizeRequest
{
    public string Url { get; set; } = string.Empty;
}

public class Base64VectorizeRequest
{
    public string Base64 { get; set; } = string.Empty;
}

public class VectorizeResponse
{
    public List<float> Vector { get; set; } = [];
    public int Dimensions { get; set; }
}

public class VectorizeProvider : IVectorizeProvider
{
    private readonly HttpClient _httpClient;

    public VectorizeProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> VectorizeImageAsync(string imageUrl)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/vectorize", new VectorizeRequest { Url = imageUrl });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VectorizeResponse>()
            ?? throw new Exception("Empty response from vectorization service");

        return [.. result.Vector];
    }

    public async Task<float[]> VectorizeImageBase64Async(string imageBase64)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/vectorize-base64", new Base64VectorizeRequest { Base64 = imageBase64 });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VectorizeResponse>()
            ?? throw new Exception("Empty response from vectorization service");

        return [.. result.Vector];
    }
}