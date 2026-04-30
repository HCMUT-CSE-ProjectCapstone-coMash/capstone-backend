using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Services;

namespace Capstone.Application.Services.ProductVectorService;

public class ProductVectorService : IProductVectorService
{
    private readonly IVectorStoreProvider _vectorStoreProvider;

    public ProductVectorService(IVectorStoreProvider vectorStoreProvider)
    {
        _vectorStoreProvider = vectorStoreProvider;
    }

    public async Task<Result<string>> InsertImageAsync(string imageUrl, string productId)
    {
        var metadata = new { product_id = productId };

        var vectorId = await _vectorStoreProvider.InsertImageAsync(imageUrl, metadata);

        return Result<string>.Success(vectorId);
    }
}