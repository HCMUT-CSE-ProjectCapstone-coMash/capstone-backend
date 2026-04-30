using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.FileStorageService;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.Products;

public class ProductsService : IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;
    private readonly IProductsOrdersRepository _productsOrdersRepository;
    private readonly IProductsOrdersDetailsRepository _productsOrdersDetailsRepository;
    private readonly IProductsOrdersDetailsQuantityChangesRepository _productsOrdersDetailsQuantityChangesRepository;
    private readonly ISaleOrderDetailsRepository _saleOrderDetailsRepository;

    private readonly IFileStorageService _fileStorageService;

    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPromptProvider _promptProvider;

    public ProductsService(
        IProductsRepository productsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IProductsOrdersRepository productsOrdersRepository,
        IProductsOrdersDetailsRepository productsOrdersDetailsRepository,
        IProductsOrdersDetailsQuantityChangesRepository productsOrdersDetailsQuantityChangesRepository,
        ISaleOrderDetailsRepository saleOrderDetailsRepository,
        IDateTimeProvider dateTimeProvider,
        IFileStorageService fileStorageService,
        IPromptProvider promptProvider
    )
    {
        _productsRepository = productsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _productsOrdersRepository = productsOrdersRepository;
        _productsOrdersDetailsRepository = productsOrdersDetailsRepository;
        _productsOrdersDetailsQuantityChangesRepository = productsOrdersDetailsQuantityChangesRepository;
        _saleOrderDetailsRepository = saleOrderDetailsRepository;
        _dateTimeProvider = dateTimeProvider;
        _fileStorageService = fileStorageService;
        _promptProvider = promptProvider;
    }

    // Tạo sản phẩm mới
    public async Task<Result<string>> CreateProduct(
        string productName,
        string category,
        string color,
        string pattern,
        string sizeType,
        string createdBy
    )
    {
        var prefix = GetCategoryPrefix(category);
        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);
        var productId = $"{prefix}-{maxNumber + 1}";

        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            Category = category,
            Color = color,
            Pattern = pattern,
            SizeType = sizeType,
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedBy = Guid.Parse(createdBy),
            Status = ProductStatus.Pending
        };

        await _productsRepository.AddProduct(newProduct);

        return Result<string>.Success(newProduct.Id.ToString());
    }

    public async Task<Result> UpdateProductImageKey(string productId, string imageKey, string vectorId)
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(productId));

        if (product == null)
            return Result.Failure(new Error("ProductNotFound", "Product not found."));

        product.ImageKey = imageKey;
        product.VectorId = vectorId;

        await _productsRepository.UpdateProduct(product);

        return Result.Success();
    }

    public async Task<Result<ProductDto>> FetchProductById(string id)
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(id));

        if (product == null)
            return Result<ProductDto>.Failure(new Error("ProductNotFound", "Product not found."));

        var imageUrl = "";
        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
        }

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
            imageUrl,
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

        var productDtos = new List<ProductWithOrderStatusDto>();

        foreach (var product in products)
        {
            var imageUrl = "";
            if (!string.IsNullOrEmpty(product.ImageKey))
            {
                var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
                imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
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
                imageUrl,
                product.VectorId,
                product.SalePrice,
                product.ImportPrice,
                productIdsInPendingOrders.Contains(product.Id)
            ));
        }

        return Result<List<ProductWithOrderStatusDto>>.Success(productDtos);
    }

    public async Task<Result<string>> CreateProductIdByCategory(string category)
    {
        var prefix = GetCategoryPrefix(category);
        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);

        var newProductId = $"{prefix}-{maxNumber + 1}";

        return Result<string>.Success(newProductId);
    }

    public async Task<Result<string>> OwnerCreateProduct(
        string productName,
        string category,
        string color,
        string pattern,
        string sizeType,
        string createdBy,
        double salePrice,
        double importPrice
    )
    {
        var prefix = GetCategoryPrefix(category);
        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);
        var productId = $"{prefix}-{maxNumber + 1}";
        
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            Category = category,
            Color = color,
            Pattern = pattern,
            SizeType = sizeType,
            CreatedBy = Guid.Parse(createdBy),
            CreatedAt = _dateTimeProvider.UtcNow,
            Status = ProductStatus.Approved,
            SalePrice = salePrice,
            ImportPrice = importPrice
        };

        await _productsRepository.AddProduct(product);

        return Result<string>.Success(product.Id.ToString());
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

        var imageUrl = "";
        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
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
            imageUrl,
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
        ));
    }

    public async Task<Result<PaginatedResult<ProductDto>>> FetchAllProducts(int currentPage, int pageSize, string? category = null, string? search = null)
    {
        var (products, total) = await _productsRepository.FetchAllProducts(currentPage, pageSize, category, search);

        var productDtos = new List<ProductDto>();

        foreach (var product in products)
        {
            var imageUrl = "";
            if (!string.IsNullOrEmpty(product.ImageKey))
            {
                var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
                imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
            }

            productDtos.Add(new ProductDto(
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
                imageUrl,
                product.VectorId,
                product.SalePrice,
                product.ImportPrice
            ));
        }

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

        var imageUrl = "";
        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
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
            imageUrl,
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

        var imageUrl = "";
        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
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
            imageUrl,
            product.VectorId,
            product.SalePrice,
            product.ImportPrice
        );

        var quantityChanges = newQuantityChange.Select(c => new ProductQuantityChangeDto(c.Size, c.OldQuantity, c.NewQuantity)).ToList();

        return Result<ProductWithQuantityChangesDto>.Success(new ProductWithQuantityChangesDto(productDto, quantityChanges));
    }

    public async Task<Result<string>> DeleteProduct(string id)
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(id));
        if (product == null)
        {
            return Result<string>.Failure(new Error("NotFound", "Product not found."));
        }

        var isInSaleOrder = await _saleOrderDetailsRepository.ExistsByProductId(product.Id);
        var isInProductsOrder = await _productsOrdersDetailsRepository.ExistsByProductId(product.Id);

        if (isInSaleOrder || isInProductsOrder)
        {
            product.Status = ProductStatus.Deleted;
            await _productsRepository.UpdateProduct(product);
        }
        else
        {
            await _productsRepository.DeleteProductAsync(product.Id);
        }

        return Result<string>.Success(product.ProductName);
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