using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.ProductVectorService;

public class ProductVectorService : IProductVectorService
{
    private readonly IVectorStoreProvider _vectorStoreProvider;
    private readonly IProductsOrdersRepository _productsOrdersRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IFileStorageService _fileStorageService;

    public ProductVectorService(
        IVectorStoreProvider vectorStoreProvider,
        IProductsOrdersRepository productsOrdersRepository,
        IProductsRepository productsRepository,
        IFileStorageService fileStorageService
    )
    {
        _vectorStoreProvider = vectorStoreProvider;
        _productsOrdersRepository = productsOrdersRepository;
        _productsRepository = productsRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<string>> InsertImageAsync(string imageUrl, string productId)
    {
        var metadata = new { product_id = productId };

        var vectorId = await _vectorStoreProvider.InsertImageAsync(imageUrl, metadata);

        return Result<string>.Success(vectorId);
    }

    public async Task<Result> DeleteImageAsync(string vectorId)
    {
        await _vectorStoreProvider.DeleteImageAsync(vectorId);
        return Result.Success();
    }

    public async Task<Result<List<ProductWithOrderStatusDto>>> FetchSimilarProducts(string imageBase64)
    {
        var base64Data = imageBase64.Contains(",") ? imageBase64.Split(',')[1] : imageBase64;

        var searchResults = await _vectorStoreProvider.SearchImageAsync(base64Data);

        var filteredResults = searchResults?.Where(r => r.Score >= 0.6f).ToList();

        if (filteredResults == null || filteredResults.Count == 0)
        {
            return Result<List<ProductWithOrderStatusDto>>.Success(new List<ProductWithOrderStatusDto>());
        }

        var productIdsInPendingOrders = await _productsOrdersRepository.GetProductIdsInPendingAndSendingOrders();

        var productDtos = new List<ProductWithOrderStatusDto>();

        foreach (var result in filteredResults)
        {
            var productId = result.Metadata.ProductId;

            var product = await _productsRepository.GetProductById(Guid.Parse(productId));

            if (product == null)
            {
                continue;
            }

            var productImageUrl = "";
            if (!string.IsNullOrEmpty(product.ImageKey))
            {
                var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
                productImageUrl = imageResult.IsSuccess ? imageResult.Value : "";
            }

            productDtos.Add(new ProductWithOrderStatusDto(
                product.Id,
                product.ProductId,
                product.ProductName,
                product.Category,
                product.Color,
                product.Pattern,
                product.SizeType,
                product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
                product.CreatedBy,
                product.CreatedAt,
                product.Status,
                productImageUrl,
                product.VectorId,
                product.SalePrice,
                product.ImportPrice,
                productIdsInPendingOrders.Contains(product.Id)
            ));
        }

        return Result<List<ProductWithOrderStatusDto>>.Success(productDtos);
    }
}