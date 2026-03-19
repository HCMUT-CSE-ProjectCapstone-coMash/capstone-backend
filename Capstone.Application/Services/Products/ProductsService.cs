using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Capstone.Application.Services.Products;

public class ProductsService : IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IVectorStoreProvider _vectorStoreProvider;

    public ProductsService(
        IProductsRepository productsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IDateTimeProvider dateTimeProvider,
        IFileStorageProvider fileStorageProvider,
        IVectorStoreProvider vectorStoreProvider
    )
    {
        _productsRepository = productsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _dateTimeProvider = dateTimeProvider;
        _fileStorageProvider = fileStorageProvider;
        _vectorStoreProvider = vectorStoreProvider;
    }

    // Tạo sản phẩm mới
    public async Task<Result<ProductDto>> CreateProduct(
        string productID,
        string productName,
        string category,
        string color,
        string? pattern,
        string sizeType,
        List<ProductQuantityDto> quantities,
        string createdBy,
        IFormFile? image
    )
    {
        var newProductID = Guid.NewGuid();
        string imageKey = "";
        string imageUrl = "";
        string vectorId = "";

        if (image != null)
        {
            var extension = Path.GetExtension(image.FileName);

            await _fileStorageProvider.UploadImageAsync(
                newProductID,
                image.OpenReadStream(),
                image.ContentType,
                extension
            );

            imageKey = $"products/{newProductID}{extension}";

            imageUrl = await _fileStorageProvider.GetImageUrlAsync(imageKey);

            vectorId = await _vectorStoreProvider.InsertImageAsync(imageUrl, new
            {
                Id = newProductID.ToString(),
            });
        }

        var product = new Product
        {
            Id = newProductID,
            ProductID = productID,
            ProductName = productName,
            Category = category,
            Color = color,
            Pattern = pattern ?? string.Empty,
            SizeType = sizeType,
            CreatedBy = Guid.Parse(createdBy),
            CreatedAt = _dateTimeProvider.UtcNow,
            Status = ProductStatus.Pending,
            ImageKey = imageKey,
            VectorId = vectorId
        };

        await _productsRepository.AddProduct(product);

        var productQuantities = new List<ProductQuantities>();

        for (int i = 0; i < quantities.Count; i++)
        {
            var quantity = quantities[i];

            var productQuantity = new ProductQuantities
            {
                Id = Guid.NewGuid(),
                ProductID = product.Id,
                Size = quantity.Size,
                Quantities = quantity.Quantities
            };

            await _productQuantitiesRepository.AddProductQuantities(productQuantity);

            productQuantities.Add(productQuantity);
        }

        return Result<ProductDto>.Success(new ProductDto(
            product.Id,
            product.ProductID,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            productQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            imageUrl,
            product.VectorId
        ));
    }

    // Tìm kiếm sản phẩm tương tự dựa trên hình ảnh
    public async Task<Result<ProductDto>> SearchProductSimilar(string ImageBase64)
    {
        var similarProducts = await _vectorStoreProvider.SearchSimilarProductsAsync(ImageBase64);

        if (similarProducts.Count == 0)
            return Result<ProductDto>.Failure(new Error("NoSimilarProducts", "No similar products found."));

        var mostSimilarProduct = similarProducts.Where(p => p.Metadata != null).MaxBy(p => p.Score);

        if (mostSimilarProduct?.Metadata == null)
            return Result<ProductDto>.Failure(new Error("NoSimilarProducts", "No similar products found."));

        var product = await _productsRepository.GetProductById(Guid.Parse(mostSimilarProduct.Metadata.Id));

        if (product == null)
            return Result<ProductDto>.Failure(new Error("ProductNotFound", "The similar product was not found."));

        return Result<ProductDto>.Success(new ProductDto(
            product.Id,
            product.ProductID,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            string.IsNullOrEmpty(product.ImageKey) ? "" : _fileStorageProvider.GetImageUrlAsync(product.ImageKey).Result,
            product.VectorId
        ));
    }

    // Lấy danh sách sản phẩm đang chờ duyệt của người dùng
    public async Task<Result<List<ProductDto>>> GetPendingProductsByUserId(Guid userId)
    {
        var products = await _productsRepository.GetPendingProductsByUserId(userId);

        var productDtos = products.Select(product => new ProductDto(
            product.Id,
            product.ProductID,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            string.IsNullOrEmpty(product.ImageKey) ? "" : _fileStorageProvider.GetImageUrlAsync(product.ImageKey).Result,
            product.VectorId
        )).ToList();

        return Result<List<ProductDto>>.Success(productDtos);
    }

    // Xóa sản phẩm theo ID
    public async Task<Result<string>> DeleteProductById(Guid productId)
    {
        var product = await _productsRepository.GetProductById(productId);

        if (product == null)
            return Result<string>.Failure(new Error("ProductNotFound", "The product was not found."));

        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            await _fileStorageProvider.DeleteImageAsync(product.ImageKey);
        }

        if (!string.IsNullOrEmpty(product.VectorId))
        {
            await _vectorStoreProvider.DeleteImageAsync(product.VectorId);
        }

        await _productsRepository.DeleteProductAsync(product.Id);

        return Result<string>.Success(product.Id.ToString());
    }
}