using System.Text.Json;
using System.Text.Json.Serialization;

namespace Capstone.Application.Common.Interfaces.Services;

public interface IVectorStoreProvider
{
    Task<string> InsertImageAsync(string imageUrl, object metadata);
    Task<List<VectorSearchResult>> SearchSimilarProductsAsync(string ImageBase64);
    Task DeleteImageAsync(string vectorId);
}

public record VectorSearchResult(
    [property: JsonPropertyName("vector_id")] string VectorId,
    [property: JsonPropertyName("score")] float Score,
    [property: JsonPropertyName("metadata")] VectorMetadata? Metadata
);

public record VectorMetadata(
    [property: JsonPropertyName("id")] string Id
);