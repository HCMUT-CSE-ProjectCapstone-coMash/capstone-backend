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
    private readonly IProductsOrdersRepository _productsOrdersRepository;
    private readonly IProductsOrdersDetailsRepository _productsOrdersDetailsRepository;
    private readonly IProductsOrdersDetailsQuantityChangesRepository _productsOrdersDetailsQuantityChangesRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IVectorStoreProvider _vectorStoreProvider;
    private readonly IPromptProvider _promptProvider;

    public ProductsService(
        IProductsRepository productsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IProductsOrdersRepository productsOrdersRepository,
        IProductsOrdersDetailsRepository productsOrdersDetailsRepository,
        IProductsOrdersDetailsQuantityChangesRepository productsOrdersDetailsQuantityChangesRepository,
        IDateTimeProvider dateTimeProvider,
        IFileStorageProvider fileStorageProvider,
        IVectorStoreProvider vectorStoreProvider,
        IPromptProvider promptProvider
    )
    {
        _productsRepository = productsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _productsOrdersRepository = productsOrdersRepository;
        _productsOrdersDetailsRepository = productsOrdersDetailsRepository;
        _productsOrdersDetailsQuantityChangesRepository = productsOrdersDetailsQuantityChangesRepository;
        _dateTimeProvider = dateTimeProvider;
        _fileStorageProvider = fileStorageProvider;
        _vectorStoreProvider = vectorStoreProvider;
        _promptProvider = promptProvider;
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

            // vectorId = await _vectorStoreProvider.InsertImageAsync(imageUrl, new
            // {
            //     Id = newProductID.ToString(),
            // });
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
            VectorId = vectorId,
            SalePrice = 0,
            ImportPrice = 0
        };

        await _productsRepository.AddProduct(product);

        var productQuantities = new List<ProductQuantity>();

        for (int i = 0; i < quantities.Count; i++)
        {
            var quantity = quantities[i];

            var productQuantity = new ProductQuantity
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
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
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
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
        ));
    }

    public async Task<Result<AnalyzeProductDto>> AnalyzeImage(string ImageBase64)
    {
        string[] AllowedCategories = ["Đầm", "Áo", "Quần", "Váy"];
        string[] AllowedColors = ["Đỏ", "Đen", "Trắng", "Cam", "Vàng", "Xanh Lá", "Xanh Dương", "Tím", "Hồng", "Nâu"];
        string[] AllowedPatterns = ["Trơn", "Sọc Dọc", "Sọc Ngang", "Caro", "Hoa Văn"];

        var analyzedProduct = await _promptProvider.AnalyzeImage(ImageBase64, AllowedCategories, AllowedColors, AllowedPatterns);

        var prefix = GetCategoryPrefix(analyzedProduct.Category);
        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);
        var productId = $"{prefix}-{maxNumber + 1}";

        var productName = analyzedProduct.Category + " " + analyzedProduct.Color + " " + analyzedProduct.Pattern;

        return Result<AnalyzeProductDto>.Success(new AnalyzeProductDto(
            productId,
            productName,
            analyzedProduct.Category,
            analyzedProduct.Color,
            analyzedProduct.Pattern
        ));
    }

    public async Task<Result<List<ProductWithOrderStatusDto>>> FetchApprovedProductByName(string productName)
    {
        var products = await _productsRepository.FetchApprovedProductByName(productName);

        var productIdsInPendingOrders = await _productsOrdersRepository.GetProductIdsInPendingAndSendingOrders();

        var productDtos = products.Select(product => new ProductWithOrderStatusDto(
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
            product.VectorId,
            product.SalePrice,
            product.ImportPrice,
            productIdsInPendingOrders.Contains(product.Id)
        )).ToList();

        return Result<List<ProductWithOrderStatusDto>>.Success(productDtos);
    }

    public async Task<Result<string>> CreateProductIdByCategory(string category)
    {
        var prefix = GetCategoryPrefix(category);
        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);

        var newProductId = $"{prefix}-{maxNumber + 1}";

        return Result<string>.Success(newProductId);
    }

    public async Task<Result<ProductDto>> OwnerCreateProduct(
        string productId,
        string productName,
        string category,
        string color,
        string? pattern,
        string sizeType,
        List<ProductQuantityDto> quantities,
        string createdBy,
        IFormFile? image,
        double salePrice,
        double importPrice
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

            // vectorId = await _vectorStoreProvider.InsertImageAsync(imageUrl, new
            // {
            //     Id = newProductID.ToString(),
            // });
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
            Status = ProductStatus.Approved,
            ImageKey = imageKey,
            VectorId = vectorId,
            SalePrice = salePrice,
            ImportPrice = importPrice
        };

        await _productsRepository.AddProduct(product);

        var productQuantities = new List<ProductQuantity>();

        for (int i = 0; i < quantities.Count; i++)
        {
            var quantity = quantities[i];

            var productQuantity = new ProductQuantity
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Size = quantity.Size,
                Quantities = quantity.Quantities
            };

            await _productQuantitiesRepository.AddProductQuantities(productQuantity);

            productQuantities.Add(productQuantity);
        }

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
            product.VectorId,
            salePrice,
            importPrice
        ));
    }

    public async Task<Result<ProductDto>> OwnerPatchProduct(
        string id,
        string? productId,
        string? productName,
        string? category,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? quantities,
        double? salePrice,
        double? importPrice
    )
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(id));

        if (product is null)
        {
            return Result<ProductDto>.Failure(new Error("NotFound", "Product not found."));
        }

        if (!string.IsNullOrWhiteSpace(productId))
            product.ProductId = productId;

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

        if (salePrice.HasValue)
            product.SalePrice = salePrice.Value;
        
        if (importPrice.HasValue)
            product.ImportPrice = importPrice.Value;

        await _productsRepository.UpdateProduct(product);

        var updatedQuantities = product.ProductQuantities.ToList();

        if (quantities is not null)
        {
            await _productQuantitiesRepository.DeleteProductQuantitiesByProductId(product.Id);

            updatedQuantities = new List<ProductQuantity>();

            foreach (var quantity in quantities)
            {
                var productQuantity = new ProductQuantity
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Size = quantity.Size,
                    Quantities = quantity.Quantities
                };

                await _productQuantitiesRepository.AddProductQuantities(productQuantity);

                updatedQuantities.Add(productQuantity);
            }
        }

        return Result<ProductDto>.Success(new ProductDto(
            product.Id,
            product.ProductId,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            updatedQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            string.IsNullOrEmpty(product.ImageKey) ? "" : _fileStorageProvider.GetImageUrlAsync(product.ImageKey).Result,
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
        ));
    }

    public async Task<Result<PaginatedResult<ProductDto>>> FetchAllProducts(int currentPage, int pageSize, string? category = null, string? search = null)
    {
        var (products, total) = await _productsRepository.FetchAllProducts(currentPage, pageSize, category, search);

        var productDtos = products.Select(product => new ProductDto(
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
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
        )).ToList();

        return Result<PaginatedResult<ProductDto>>.Success(
            new PaginatedResult<ProductDto>(productDtos, total));
    }

    public async Task<Result<ProductWithQuantityChangesDto>> OwnerUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? newQuantities,
        double? salePrice,
        double? importPrice
    )
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(id));

        if (product is null)
        {
            return Result<ProductWithQuantityChangesDto>.Failure(new Error("NotFound", "Product not found."));
        }

        if (!string.IsNullOrWhiteSpace(productName))
            product.ProductName = productName;

        if (!string.IsNullOrWhiteSpace(color))
            product.Color = color;

        if (pattern is not null)
            product.Pattern = pattern;

        if (!string.IsNullOrWhiteSpace(sizeType))
            product.SizeType = sizeType;

        if (salePrice.HasValue)
            product.SalePrice = salePrice.Value;

        if (importPrice.HasValue)
            product.ImportPrice = importPrice.Value;

        await _productsRepository.UpdateProduct(product);

        List<ProductQuantity> newQuantity = new();
        List<ProductsOrdersDetailQuantityChange> newQuantityChange = new();

        if (product.Status == ProductStatus.Approved && newQuantities != null)
        {
            var existingDetail = await _productsOrdersDetailsRepository.GetProductsOrdersDetailsByOrderIdAndProductId(Guid.Parse(productsOrderId), Guid.Parse(id));

            ProductsOrdersDetail detail;

            if (existingDetail is not null)
            {
                detail = existingDetail;

                await _productsOrdersDetailsQuantityChangesRepository.DeleteQuantityChangesByProductsOrdersDetailId(detail.Id);
            }
            else
            {
                detail = new ProductsOrdersDetail
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductsOrderId = Guid.Parse(productsOrderId),
                };

                await _productsOrdersDetailsRepository.CreateProductsOrdersDetails(detail);
            }

            var allSizes = product.ProductQuantities.Select(q => q.Size).Union(newQuantities.Select(q => q.Size));

            foreach (var size in allSizes)
            {
                var currentQuantity = product.ProductQuantities.FirstOrDefault(q => q.Size == size);
                var requestedQuantity = newQuantities.FirstOrDefault(q => q.Size == size);

                if (requestedQuantity is null) continue;

                var oldQty = currentQuantity?.Quantities ?? 0;
                var newQty = requestedQuantity.Quantities;

                if (oldQty == newQty) continue;

                var newQuantityChane = new ProductsOrdersDetailQuantityChange
                {
                    Id = Guid.NewGuid(),
                    ProductsOrdersDetailId = detail.Id,
                    Size = size,
                    OldQuantity = oldQty,
                    NewQuantity = newQty,
                };

                await _productsOrdersDetailsQuantityChangesRepository.AddQuantityChange(newQuantityChane);

                newQuantityChange.Add(newQuantityChane);
            }
        }

        if (product.Status == ProductStatus.Pending && newQuantities != null)
        {
            await _productQuantitiesRepository.DeleteProductQuantitiesByProductId(product.Id);

            foreach (var quantity in newQuantities)
            {
                var productQuantity = new ProductQuantity
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Size = quantity.Size,
                    Quantities = quantity.Quantities
                };

                await _productQuantitiesRepository.AddProductQuantities(productQuantity);

                newQuantity.Add(productQuantity);
            }
        }

        var productDto = new ProductDto(
            product.Id,
            product.ProductId,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            newQuantity.Count > 0 ? newQuantity.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList() : product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            string.IsNullOrEmpty(product.ImageKey) ? "" : _fileStorageProvider.GetImageUrlAsync(product.ImageKey).Result,
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
        );

        var quantityChanges = newQuantityChange.Select(c => new ProductQuantityChangeDto(c.Size, c.OldQuantity, c.NewQuantity)).ToList();

        return Result<ProductWithQuantityChangesDto>.Success(new ProductWithQuantityChangesDto(productDto, quantityChanges));
    }

    public async Task<Result<ProductWithQuantityChangesDto>> EmployeeUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? newQuantities
    )
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(id));

        if (product is null)
        {
            return Result<ProductWithQuantityChangesDto>.Failure(new Error("NotFound", "Product not found."));
        }

        if (!string.IsNullOrWhiteSpace(productName))
            product.ProductName = productName;

        if (!string.IsNullOrWhiteSpace(color))
            product.Color = color;

        if (pattern is not null)
            product.Pattern = pattern;

        if (!string.IsNullOrWhiteSpace(sizeType))
            product.SizeType = sizeType;

        await _productsRepository.UpdateProduct(product);

        List<ProductQuantity> newQuantity = new();
        List<ProductsOrdersDetailQuantityChange> newQuantityChange = new();

        if (product.Status == ProductStatus.Approved && newQuantities != null)
        {
            var existingDetail = await _productsOrdersDetailsRepository.GetProductsOrdersDetailsByOrderIdAndProductId(Guid.Parse(productsOrderId), Guid.Parse(id));

            ProductsOrdersDetail detail;

            if (existingDetail is not null)
            {
                detail = existingDetail;

                await _productsOrdersDetailsQuantityChangesRepository.DeleteQuantityChangesByProductsOrdersDetailId(detail.Id);
            }
            else
            {
                detail = new ProductsOrdersDetail
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductsOrderId = Guid.Parse(productsOrderId),
                };

                await _productsOrdersDetailsRepository.CreateProductsOrdersDetails(detail);
            }

            var allSizes = product.ProductQuantities.Select(q => q.Size).Union(newQuantities.Select(q => q.Size));

            foreach (var size in allSizes)
            {
                var currentQuantity = product.ProductQuantities.FirstOrDefault(q => q.Size == size);
                var requestedQuantity = newQuantities.FirstOrDefault(q => q.Size == size);

                if (requestedQuantity is null) continue;

                var oldQty = currentQuantity?.Quantities ?? 0;
                var newQty = requestedQuantity.Quantities;

                if (oldQty == newQty) continue;

                var newQuantityChane = new ProductsOrdersDetailQuantityChange
                {
                    Id = Guid.NewGuid(),
                    ProductsOrdersDetailId = detail.Id,
                    Size = size,
                    OldQuantity = oldQty,
                    NewQuantity = newQty,
                };

                await _productsOrdersDetailsQuantityChangesRepository.AddQuantityChange(newQuantityChane);

                newQuantityChange.Add(newQuantityChane);
            }
        }

        if (product.Status == ProductStatus.Pending && newQuantities != null)
        {
            await _productQuantitiesRepository.DeleteProductQuantitiesByProductId(product.Id);

            foreach (var quantity in newQuantities)
            {
                var productQuantity = new ProductQuantity
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Size = quantity.Size,
                    Quantities = quantity.Quantities
                };

                await _productQuantitiesRepository.AddProductQuantities(productQuantity);

                newQuantity.Add(productQuantity);
            }
        }

        var productDto = new ProductDto(
            product.Id,
            product.ProductId,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            newQuantity.Count > 0 ? newQuantity.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList() : product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            string.IsNullOrEmpty(product.ImageKey) ? "" : _fileStorageProvider.GetImageUrlAsync(product.ImageKey).Result,
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
        );

        var quantityChanges = newQuantityChange.Select(c => new ProductQuantityChangeDto(c.Size, c.OldQuantity, c.NewQuantity)).ToList();

        return Result<ProductWithQuantityChangesDto>.Success(new ProductWithQuantityChangesDto(productDto, quantityChanges));
    }

    private static string GetCategoryPrefix(string category) => category switch
    {
        "Váy" => "VAY",
        "Đầm" => "DAM",
        "Áo" => "AO",
        "Quần" => "QUAN",
        _ => "PRD"
    };
}