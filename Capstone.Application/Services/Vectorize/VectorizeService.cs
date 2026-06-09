using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.Vectorize;

public class VectorizeService : IVectorizeService
{
    private readonly IVectorizeProvider _vectorizeProvider;
    private readonly IProductsRepository _productsRepository;
    private readonly IProductsOrdersRepository _productsOrdersRepository;

    private readonly IFileStorageService _fileStorageService;

    public VectorizeService(
        IVectorizeProvider vectorizeProvider,
        IProductsRepository productsRepository,
        IProductsOrdersRepository productsOrdersRepository,
        IFileStorageService fileStorageService
    )
    {
        _vectorizeProvider = vectorizeProvider;
        _productsRepository = productsRepository;
        _productsOrdersRepository = productsOrdersRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<float[]>> VectorizeImageAsync(string imageUrl)
    {
        var vector = await _vectorizeProvider.VectorizeImageAsync(imageUrl);

        return Result<float[]>.Success(vector);
    }

    public async Task<Result<List<ProductWithOrderStatusDto>>> FetchSimilarProducts(string imageBase64)
    {
        var vectorResult = await _vectorizeProvider.VectorizeImageBase64Async(imageBase64);
        if (vectorResult == null || vectorResult.Length == 0)
            return Result<List<ProductWithOrderStatusDto>>.Success(new List<ProductWithOrderStatusDto>());

        var similarProducts = await _productsRepository.FetchSimilarProductsByVector(vectorResult, topK: 10);

        if (similarProducts.Count == 0)
            return Result<List<ProductWithOrderStatusDto>>.Success(new List<ProductWithOrderStatusDto>());

        var productIdsInPendingOrders = await _productsOrdersRepository.GetProductIdsInPendingAndSendingOrders();

        var productDtos = new List<ProductWithOrderStatusDto>();

        foreach (var product in similarProducts)
        {
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
                product.Category.CategoryName,
                product.Color.ColorName,
                product.Pattern.PatternName,
                product.SizeType,
                product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
                product.CreatedBy,
                product.CreatedAt,
                product.Status,
                productImageUrl,
                product.SalePrice,
                product.ImportPrice,
                productIdsInPendingOrders.Contains(product.Id)
            ));
        }

        return Result<List<ProductWithOrderStatusDto>>.Success(productDtos);
    }
}