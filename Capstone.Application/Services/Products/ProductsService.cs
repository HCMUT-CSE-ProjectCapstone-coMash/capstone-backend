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

    public ProductsService(
        IProductsRepository productsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IDateTimeProvider dateTimeProvider,
        IFileStorageProvider fileStorageProvider
    )
    {
        _productsRepository = productsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _dateTimeProvider = dateTimeProvider;
        _fileStorageProvider = fileStorageProvider;
    }

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
            ImageKey = imageKey
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
            imageUrl
        ));
    }

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
            string.IsNullOrEmpty(product.ImageKey) ? "" : _fileStorageProvider.GetImageUrlAsync(product.ImageKey).Result
        )).ToList();

        return Result<List<ProductDto>>.Success(productDtos);  
    }

    public async Task<Result<ProductDto>> UpdateProduct(
        Guid productId,
        string? productID,
        string? productName,
        string? category,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? quantities
    )
    {
        var product = await _productsRepository.GetProductById(productId);
        if (product is null)
        {
            return Result<ProductDto>.Failure(new Error("NotFound", "Product not found."));
        }

        // Ensure CreatedAt always has a UTC kind before saving 
        product.CreatedAt = DateTime.SpecifyKind(product.CreatedAt, DateTimeKind.Utc);

        if (!string.IsNullOrWhiteSpace(productID))
            product.ProductID = productID;

        if (!string.IsNullOrWhiteSpace(productName))
            product.ProductName = productName;

        if (!string.IsNullOrWhiteSpace(category))
            product.Category = category;

        if (!string.IsNullOrWhiteSpace(color))
            product.Color = color;

        if (pattern is not null)
            product.Pattern = pattern;

        if (!string.IsNullOrWhiteSpace(sizeType))
            product.SizeType = sizeType;

        await _productsRepository.UpdateProduct(product);

        var updatedQuantities = product.ProductQuantities.ToList();

        if (quantities is not null)
        {
            await _productQuantitiesRepository.DeleteByProductId(product.Id);

            updatedQuantities = new List<ProductQuantities>();

            foreach (var quantity in quantities)
            {
                var productQuantity = new ProductQuantities
                {
                    Id = Guid.NewGuid(),
                    ProductID = product.Id,
                    Size = quantity.Size,
                    Quantities = quantity.Quantities
                };

                await _productQuantitiesRepository.AddProductQuantities(productQuantity);
                updatedQuantities.Add(productQuantity);
            }
        }

        var imageUrl = string.IsNullOrEmpty(product.ImageKey)
            ? string.Empty
            : await _fileStorageProvider.GetImageUrlAsync(product.ImageKey);

        return Result<ProductDto>.Success(new ProductDto(
            product.Id,
            product.ProductID,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            updatedQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            imageUrl
        ));
    }
}