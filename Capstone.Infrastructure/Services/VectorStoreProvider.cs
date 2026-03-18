using Capstone.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;

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

    public Task<string> InsertImageAsync(Stream imageStream, object metadata)
    {           
        return Task.FromResult("vector-id-placeholder");
    }
}