using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.Products;

public class ProductsService : IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProductsService(
        IProductsRepository productsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IDateTimeProvider dateTimeProvider
    )
    {
        _productsRepository = productsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ProductDto>> CreateProduct(
        string productID,
        string productName,
        string category,
        string color,
        string pattern,
        string sizeType,
        List<ProductQuantityDto> quantities,
        string createdBy
    )
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ProductID = productID,
            ProductName = productName,
            Category = category,
            Color = color,
            Pattern = pattern,
            SizeType = sizeType,
            CreatedBy = Guid.Parse(createdBy),
            CreatedAt = _dateTimeProvider.UtcNow,
            Status = ProductStatus.Pending
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
            product.Status
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
            product.Status
        )).ToList();

        return Result<List<ProductDto>>.Success(productDtos);  
    } 
}