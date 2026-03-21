using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.Products;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.ProductsOrders;

public class ProductsOrdersService : IProductsOrdersService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IVectorStoreProvider _vectorStoreProvider;
    private readonly IProductsOrdersRepository _productsOrdersRepository;
    private readonly IProductsOrdersDetailsRepository _productsOrdersDetailsRepository;
    private readonly IProductsRepository _productsRepository;



    public ProductsOrdersService(
        IDateTimeProvider dateTimeProvider,
        IFileStorageProvider fileStorageProvider,
        IVectorStoreProvider vectorStoreProvider,
        IProductsOrdersRepository productsOrdersRepository,
        IProductsOrdersDetailsRepository productsOrdersDetailsRepository,
        IProductsRepository productsRepository
    )
    {
        _dateTimeProvider = dateTimeProvider;
        _fileStorageProvider = fileStorageProvider;
        _vectorStoreProvider = vectorStoreProvider;
        _productsOrdersRepository = productsOrdersRepository;
        _productsOrdersDetailsRepository = productsOrdersDetailsRepository;
        _productsRepository = productsRepository;
    }

    public async Task<Result<ProductsOrdersDto>> FetchOrCreateProductsOrders(string createdBy)
    {
        var productsOrders = await _productsOrdersRepository.GetProductsOrdersByCreatedByAndStatus(Guid.Parse(createdBy), ProductsOrderStatus.Pending);

        if (productsOrders != null)
        {
            var productsOrdersDto = new ProductsOrdersDto(
                Id: productsOrders.Id,
                CreatedBy: productsOrders.CreatedBy,
                CreatedAt: productsOrders.CreatedAt,
                OrderName: productsOrders.OrderName,
                OrderDescription: productsOrders.OrderDescription,
                OrderStatus: productsOrders.OrderStatus,
                Products: productsOrders.ProductsOrdersDetails.Select(detail => new ProductDto
                (
                    Id: detail.Product.Id,
                    ProductId: detail.Product.ProductId,
                    ProductName: detail.Product.ProductName,
                    Category: detail.Product.Category,
                    Color: detail.Product.Color,
                    Pattern: detail.Product.Pattern,
                    SizeType: detail.Product.SizeType,
                    Quantities: detail.Product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
                    CreatedBy: detail.Product.CreatedBy,
                    CreatedAt: detail.Product.CreatedAt,
                    Status: detail.Product.Status,
                    ImageURL: detail.Product.ImageKey != null ? _fileStorageProvider.GetImageUrlAsync(detail.Product.ImageKey).Result : "",
                    VectorId: detail.Product.VectorId
                )).ToList()
            );

            return Result<ProductsOrdersDto>.Success(productsOrdersDto);
        }

        var newProductsOrder = new ProductsOrder
        {
            Id = Guid.NewGuid(),
            CreatedBy = Guid.Parse(createdBy),
            CreatedAt = _dateTimeProvider.UtcNow,
            OrderName = string.Empty,
            OrderDescription = string.Empty,
            OrderStatus = ProductsOrderStatus.Pending
        };

        await _productsOrdersRepository.CreateProductsOrders(newProductsOrder);

        var newProductsOrdersDto = new ProductsOrdersDto(
            Id: newProductsOrder.Id,
            CreatedBy: newProductsOrder.CreatedBy,
            CreatedAt: newProductsOrder.CreatedAt,
            OrderName: newProductsOrder.OrderName,
            OrderDescription: newProductsOrder.OrderDescription,
            OrderStatus: newProductsOrder.OrderStatus,
            Products: new List<ProductDto>()
        );

        return Result<ProductsOrdersDto>.Success(newProductsOrdersDto);
    }

    public async Task<Result<string>> DeleteProductFromProductsOrders(string orderId, string productId)
    {
        // Kiếm xem có tồn tại ProductsOrdersDetail với orderId và productId hay không và xoá
        var productsOrdersDetail = await _productsOrdersDetailsRepository.GetProductsOrdersDetailsByOrderIdAndProductId(Guid.Parse(orderId), Guid.Parse(productId));

        if (productsOrdersDetail == null)
        {
            return Result<string>.Failure(new Error("ProductsOrdersDetailNotFound", "The product order detail was not found."));
        }

        await _productsOrdersDetailsRepository.DeleteProductsOrdersDetails(productsOrdersDetail.Id);

        // Kiếm xem có tồn tại Product với productId hay không và xoá
        var product = await _productsRepository.GetProductById(Guid.Parse(productId));

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