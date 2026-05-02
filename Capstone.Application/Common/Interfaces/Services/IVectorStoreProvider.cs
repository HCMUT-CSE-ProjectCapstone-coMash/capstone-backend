using System.Text.Json.Serialization;

namespace Capstone.Application.Common.Interfaces.Services;

public interface IVectorStoreProvider
{
    Task<string> InsertImageAsync(string imageUrl, object metadata);

    Task DeleteImageAsync(string vectorId);

    Task<List<SearchResult>> SearchImageAsync(string imageUrl);
}

public record ProductMetadata(
    [property: JsonPropertyName("product_id")] string ProductId
);

public record SearchResult(string VectorId, string Content, ProductMetadata Metadata, float Score);