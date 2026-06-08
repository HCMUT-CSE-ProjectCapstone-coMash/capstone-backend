using Capstone.Application.Common.Interfaces.Services;

namespace Capstone.Application.Services.Vectorize;

public class VectorizeService : IVectorizeService
{
    private readonly IVectorizeProvider _vectorizeProvider;

    public VectorizeService(IVectorizeProvider vectorizeProvider)
    {
        _vectorizeProvider = vectorizeProvider;
    }

    public async Task<float[]> VectorizeImageAsync(string imageUrl)
    {
        return await _vectorizeProvider.VectorizeImageAsync(imageUrl);
    }
}