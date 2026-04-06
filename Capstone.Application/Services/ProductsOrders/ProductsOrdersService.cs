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
    private readonly IProductsOrdersDetailsQuantityChangesRepository _productsOrdersDetailsQuantityChangesRepository;
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IUsersRepository _usersRepository;

    public ProductsOrdersService(
        IDateTimeProvider dateTimeProvider,
        IFileStorageProvider fileStorageProvider,
        IVectorStoreProvider vectorStoreProvider,
        IProductsOrdersRepository productsOrdersRepository,
        IProductsOrdersDetailsRepository productsOrdersDetailsRepository,
        IProductsOrdersDetailsQuantityChangesRepository productsOrdersDetailsQuantityChangesRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IProductsRepository productsRepository,
        IUsersRepository usersRepository
    )
    {
        _dateTimeProvider = dateTimeProvider;
        _fileStorageProvider = fileStorageProvider;
        _vectorStoreProvider = vectorStoreProvider;
        _productsOrdersRepository = productsOrdersRepository;
        _productsOrdersDetailsRepository = productsOrdersDetailsRepository;
        _productsOrdersDetailsQuantityChangesRepository = productsOrdersDetailsQuantityChangesRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _productsRepository = productsRepository;
        _usersRepository = usersRepository;
    }

    public async Task<Result<ProductsOrdersDto>> FetchOrCreateProductsOrders(string createdBy)
    {
        var productsOrders = await _productsOrdersRepository.GetProductsOrdersByCreatedBy(Guid.Parse(createdBy));

        if (productsOrders != null)
        {
            var products = new List<ProductWithQuantityChangesDto>();

            foreach (var detail in productsOrders.ProductsOrdersDetails)
            {
                var imageUrl = detail.Product.ImageKey != null ? await _fileStorageProvider.GetImageUrlAsync(detail.Product.ImageKey) : "";

                var productDto = new ProductDto(
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
                    ImageURL: imageUrl,
                    VectorId: detail.Product.VectorId,
                    SalePrice: detail.Product.SalePrice,
                    ImportPrice: detail.Product.ImportPrice
                );

                var quantityChanges = detail.QuantityChanges.Select(qc => new ProductQuantityChangeDto(qc.Size, qc.OldQuantity, qc.NewQuantity)).ToList();

                products.Add(new ProductWithQuantityChangesDto(productDto, quantityChanges));
            }

            return Result<ProductsOrdersDto>.Success(new ProductsOrdersDto(
                Id: productsOrders.Id,
                CreatedBy: productsOrders.CreatedBy,
                CreatedAt: productsOrders.CreatedAt,
                OrderName: productsOrders.OrderName,
                OrderDescription: productsOrders.OrderDescription,
                OrderStatus: productsOrders.OrderStatus,
                Products: products
            ));
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
            Products: new List<ProductWithQuantityChangesDto>()
        );

        return Result<ProductsOrdersDto>.Success(newProductsOrdersDto);
    }

    public async Task<Result<List<ProductsOrdersListDto>>> GetAllProductsOrdersExcludingPending()
    {
        var productsOrders = await _productsOrdersRepository.GetProductsOrdersExcludingPending();
        var productsOrdersDtos = new List<ProductsOrdersListDto>();

        foreach (var productsOrder in productsOrders)
        {
            var createdByUser = await _usersRepository.GetUserById(productsOrder.CreatedBy);
            var createdByName = createdByUser?.FullName ?? string.Empty;

            productsOrdersDtos.Add(new ProductsOrdersListDto(
                Id: productsOrder.Id,
                CreatedBy: productsOrder.CreatedBy,
                CreatedByName: createdByName,
                CreatedAt: productsOrder.CreatedAt,
                OrderName: productsOrder.OrderName,
                OrderDescription: productsOrder.OrderDescription,
                OrderStatus: productsOrder.OrderStatus
            ));
        }

        return Result<List<ProductsOrdersListDto>>.Success(productsOrdersDtos);
    }

    public async Task<Result<string>> DeleteProductFromProductsOrders(string orderId, string productId)
    {
        // Kiếm xem có tồn tại ProductsOrdersDetail với orderId và productId hay không và xoá
        var productsOrdersDetail = await _productsOrdersDetailsRepository.GetProductsOrdersDetailsByOrderIdAndProductId(Guid.Parse(orderId), Guid.Parse(productId));

        if (productsOrdersDetail == null)
        {
            return Result<string>.Failure(new Error("ProductsOrdersDetailNotFound", "The product order detail was not found."));
        }

        var product = await _productsRepository.GetProductById(Guid.Parse(productId));

        if (product == null)
            return Result<string>.Failure(new Error("ProductNotFound", "The product was not found."));

        if (product.Status == ProductStatus.Pending)
        {
            if (!string.IsNullOrEmpty(product.ImageKey))
            {
                await _fileStorageProvider.DeleteImageAsync(product.ImageKey);
            }

            if (!string.IsNullOrEmpty(product.VectorId))
            {
                await _vectorStoreProvider.DeleteImageAsync(product.VectorId);
            }

            await _productsRepository.DeleteProductAsync(product.Id);
        }

        await _productsOrdersDetailsRepository.DeleteProductsOrdersDetails(productsOrdersDetail.Id);

        return Result<string>.Success(product.Id.ToString());
    }

    public async Task<Result<string>> PatchProductsOrders(string orderId, string? orderName, string? OrderDescription, string? OrderStatus)
    {
        var productsOrder = await _productsOrdersRepository.GetProductsOrdersByOrderId(Guid.Parse(orderId));

        if (productsOrder is null)
        {
            return Result<string>.Failure(new Error("NotFound", "ProductsOrders not found."));
        }

        if (!string.IsNullOrWhiteSpace(orderName))
            productsOrder.OrderName = orderName;

        if (!string.IsNullOrWhiteSpace(OrderDescription))
            productsOrder.OrderDescription = OrderDescription;

        if (!string.IsNullOrWhiteSpace(OrderStatus))
            productsOrder.OrderStatus = OrderStatus;

        await _productsOrdersRepository.PatchProductsOrders(productsOrder);

        return Result<string>.Success(productsOrder.Id.ToString());
    }

    public async Task<Result<ProductsOrdersDto>> GetProductsOrderById(string orderId)
    {
        var productsOrders = await _productsOrdersRepository.GetProductsOrdersByOrderId(Guid.Parse(orderId));

        if (productsOrders == null)
        {
            return Result<ProductsOrdersDto>.Failure(new Error("NotFound", "ProductsOrders not found."));
        }

        var products = new List<ProductWithQuantityChangesDto>();

        foreach (var detail in productsOrders.ProductsOrdersDetails)
        {
            var imageUrl = detail.Product.ImageKey != null ? await _fileStorageProvider.GetImageUrlAsync(detail.Product.ImageKey) : "";

            var productDto = new ProductDto(
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
                ImageURL: imageUrl,
                VectorId: detail.Product.VectorId,
                SalePrice: detail.Product.SalePrice,
                ImportPrice: detail.Product.ImportPrice
            );

            var quantityChanges = detail.QuantityChanges.Select(qc => new ProductQuantityChangeDto(qc.Size, qc.OldQuantity, qc.NewQuantity)).ToList();

            products.Add(new ProductWithQuantityChangesDto(productDto, quantityChanges));
        }

        return Result<ProductsOrdersDto>.Success(new ProductsOrdersDto(
            Id: productsOrders.Id,
            CreatedBy: productsOrders.CreatedBy,
            CreatedAt: productsOrders.CreatedAt,
            OrderName: productsOrders.OrderName,
            OrderDescription: productsOrders.OrderDescription,
            OrderStatus: productsOrders.OrderStatus,
            Products: products
        ));
    }

    public async Task<Result<string>> ApproveProductsOrder(string orderId)
    {
        var productsOrder = await _productsOrdersRepository.GetProductsOrdersByOrderId(Guid.Parse(orderId));

        if (productsOrder is null)
        {
            return Result<string>.Failure(new Error("NotFound", "ProductsOrders not found."));
        }

        foreach (var detail in productsOrder.ProductsOrdersDetails)
        {
            var product = detail.Product;

            if (product.Status == ProductStatus.Pending)
            {
                product.Status = ProductStatus.Approved;
                await _productsRepository.UpdateProduct(product);
            }
            else if (product.Status == ProductStatus.Approved)
            {
                foreach (var quantityChange in detail.QuantityChanges)
                { 
                    var existingQuantity = product.ProductQuantities.FirstOrDefault(q => q.Size == quantityChange.Size);
                    if (existingQuantity != null)
                    {
                        existingQuantity.Quantities = quantityChange.NewQuantity;
                        await _productQuantitiesRepository.UpdateProductQuantity(existingQuantity);
                    }
                }
                await _productsOrdersDetailsQuantityChangesRepository.DeleteQuantityChangesByProductsOrdersDetailId(detail.Id);
            }
        }

        productsOrder.OrderStatus = ProductsOrderStatus.Approved;
        await _productsOrdersRepository.PatchProductsOrders(productsOrder);

        return Result<string>.Success(productsOrder.Id.ToString());
    }

    public async Task<Result<string>> DeleteProductsOrder(string orderId)
    {
        var productsOrder = await _productsOrdersRepository.GetProductsOrdersByOrderId(Guid.Parse(orderId));

        if (productsOrder is null)
        {
            return Result<string>.Failure(new Error("NotFound", "ProductsOrders not found."));
        }

        var details = productsOrder.ProductsOrdersDetails.ToList();
        foreach (var detail in details)
        {
            var product = detail.Product;

            if (product.Status == ProductStatus.Pending)
            {
                if (!string.IsNullOrEmpty(product.ImageKey))
                {
                    await _fileStorageProvider.DeleteImageAsync(product.ImageKey);
                }

                if (!string.IsNullOrEmpty(product.VectorId))
                {
                    await _vectorStoreProvider.DeleteImageAsync(product.VectorId);
                }

                await _productsRepository.DeleteProductAsync(product.Id);
            }
            else if (product.Status == ProductStatus.Approved)
            {
                await _productsOrdersDetailsRepository.DeleteProductsOrdersDetails(detail.Id);
            }
        }

        await _productsOrdersRepository.DeleteProductsOrder(productsOrder);

        return Result<string>.Success(productsOrder.Id.ToString());
    }
}