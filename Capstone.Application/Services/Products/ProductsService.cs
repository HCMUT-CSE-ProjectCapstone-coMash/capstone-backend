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
    private readonly IProductsOrdersDetailsRepository _productsOrdersDetailsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IVectorStoreProvider _vectorStoreProvider;

    public ProductsService(
        IProductsRepository productsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IProductsOrdersDetailsRepository productsOrdersDetailsRepository,
        IDateTimeProvider dateTimeProvider,
        IFileStorageProvider fileStorageProvider,
        IVectorStoreProvider vectorStoreProvider
    )
    {
        _productsRepository = productsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _productsOrdersDetailsRepository = productsOrdersDetailsRepository;
        _dateTimeProvider = dateTimeProvider;
        _fileStorageProvider = fileStorageProvider;
        _vectorStoreProvider = vectorStoreProvider;
    }

    // Tạo sản phẩm mới
    public async Task<Result<ProductDto>> CreateProduct(
        string productId,
        string productName,
        string category,
        string color,
        string? pattern,
        string sizeType,
        List<ProductQuantityDto> quantities,
        string createdBy,
        IFormFile? image,
        string orderID
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
            ProductId = productId,
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
                ProductId = product.Id,
                Size = quantity.Size,
                Quantities = quantity.Quantities
            };

            await _productQuantitiesRepository.AddProductQuantities(productQuantity);

            productQuantities.Add(productQuantity);
        }

        var newProductsOrdersDetail = new ProductsOrdersDetail
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            ProductsOrderId = Guid.Parse(orderID),
        };

        await _productsOrdersDetailsRepository.CreateProductsOrdersDetails(newProductsOrdersDetail);

        return Result<ProductDto>.Success(new ProductDto(
            product.Id,
            product.ProductId,
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
            string.IsNullOrEmpty(product.ImageKey) ? "" : _fileStorageProvider.GetImageUrlAsync(product.ImageKey).Result,
            product.VectorId
        ));
    }
}