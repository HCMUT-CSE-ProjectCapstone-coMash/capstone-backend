using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.ProductVectorService;
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
    private readonly ICategoryRepository _categoryRepository;
    private readonly IColorRepository _colorRepository;
    private readonly IPatternRepository _patternRepository;

    private readonly IFileStorageService _fileStorageService;

    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPromptProvider _promptProvider;
    private readonly IModelPromptProvider _modelPromptProvider;

    public ProductsService(
        IProductsRepository productsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IProductsOrdersRepository productsOrdersRepository,
        IProductsOrdersDetailsRepository productsOrdersDetailsRepository,
        IProductsOrdersDetailsQuantityChangesRepository productsOrdersDetailsQuantityChangesRepository,
        ISaleOrderDetailsRepository saleOrderDetailsRepository,
        ICategoryRepository categoryRepository,
        IColorRepository colorRepository,
        IPatternRepository patternRepository,
        IDateTimeProvider dateTimeProvider,
        IFileStorageService fileStorageService,
        IPromptProvider promptProvider,
        IModelPromptProvider modelPromptProvider
    )
    {
        _productsRepository = productsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _productsOrdersRepository = productsOrdersRepository;
        _productsOrdersDetailsRepository = productsOrdersDetailsRepository;
        _productsOrdersDetailsQuantityChangesRepository = productsOrdersDetailsQuantityChangesRepository;
        _saleOrderDetailsRepository = saleOrderDetailsRepository;
        _categoryRepository = categoryRepository;
        _colorRepository = colorRepository;
        _patternRepository = patternRepository;
        _dateTimeProvider = dateTimeProvider;
        _fileStorageService = fileStorageService;
        _promptProvider = promptProvider;
        _modelPromptProvider = modelPromptProvider;
    }

    // Tạo sản phẩm mới
    public async Task<Result<string>> CreateProduct(
        string productName,
        string categoryId,
        string colorId,
        string patternId,
        string sizeType,
        string createdBy
    )
    {
        var category = await _categoryRepository.GetCategoryById(Guid.Parse(categoryId));

        if (category == null)
        {
            return Result<string>.Failure(new Error("CategoryNotFound", "Category not found."));
        }

        var prefix = category.CategoryId;
        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);
        var productId = $"{prefix}-{maxNumber + 1}";

        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            CategoryId = Guid.Parse(categoryId),
            ColorId = Guid.Parse(colorId),
            PatternId = Guid.Parse(patternId),
            SizeType = sizeType,
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedBy = Guid.Parse(createdBy),
            Status = ProductStatus.Pending
        };

        await _productsRepository.AddProduct(newProduct);

        return Result<string>.Success(newProduct.Id.ToString());
    }

    public async Task<Result> UpdateProductImageKey(string productId, string imageKey, float[] vector)
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(productId));

        if (product == null)
            return Result.Failure(new Error("ProductNotFound", "Product not found."));

        product.ImageKey = imageKey;
        product.Embedding = new Pgvector.Vector(vector);

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
            product.Category.CategoryName,
            product.Color.ColorName,
            product.Pattern.PatternName,
            product.SizeType,
            product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            imageUrl,
            product.VectorId,
            product.SalePrice,
            product.ImportPrice,
            await GetModelImageUrlAsync(product.ModelImageKey)
        ));
    }

    public async Task<Result<ProductDto>> FetchProductByProductId(string productId)
    {
        var product = await _productsRepository.GetProductByProductId(productId);

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
            product.Category.CategoryName,
            product.Color.ColorName,
            product.Pattern.PatternName,
            product.SizeType,
            product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status,
            imageUrl,
            product.VectorId,
            product.SalePrice,
            product.ImportPrice,
            await GetModelImageUrlAsync(product.ModelImageKey)
        ));
    }

    public async Task<Result<AnalyzeProductDto>> AnalyzeImage(string ImageBase64)
    {
        var category = await _categoryRepository.FetchCategories();
        var color = await _colorRepository.FetchColors();
        var pattern = await _patternRepository.FetchPatterns();

        var categoryNames = category.Select(c => c.CategoryName).ToList();
        var colorNames = color.Select(c => c.ColorName).ToList();
        var patternNames = pattern.Select(p => p.PatternName).ToList();

        var analyzedProduct = await _promptProvider.AnalyzeImageWithClaude(ImageBase64, categoryNames, colorNames, patternNames);

        var categoryEntity = category.FirstOrDefault(c => c.CategoryName == analyzedProduct.Category);

        if (categoryEntity == null)
        {
            return Result<AnalyzeProductDto>.Failure(new Error("CategoryNotFound", "Category not found."));
        }

        var prefix = categoryEntity.CategoryId;
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

            var modelImageUrl = await GetModelImageUrlAsync(product.ModelImageKey);

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
                imageUrl,
                product.VectorId,
                product.SalePrice,
                product.ImportPrice,
                productIdsInPendingOrders.Contains(product.Id),
                modelImageUrl
            ));
        }

        return Result<List<ProductWithOrderStatusDto>>.Success(productDtos);
    }

    public async Task<Result<string>> CreateProductIdByCategoryId(string categoryId)
    {
        var category = await _categoryRepository.GetCategoryById(Guid.Parse(categoryId));

        if (category == null)
        {
            return Result<string>.Failure(new Error("CategoryNotFound", "Category not found."));
        }

        var prefix = category.CategoryId;

        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);

        var newProductId = $"{prefix}-{maxNumber + 1}";

        return Result<string>.Success(newProductId);
    }

    public async Task<Result<string>> OwnerCreateProduct(
        string productName,
        string categoryId,
        string colorId,
        string patternId,
        string sizeType,
        string createdBy,
        double salePrice,
        double importPrice
    )
    {
        var category = await _categoryRepository.GetCategoryById(Guid.Parse(categoryId));

        if (category == null)
        {
            return Result<string>.Failure(new Error("CategoryNotFound", "Category not found."));
        }

        var prefix = category.CategoryId;
        var maxNumber = await _productsRepository.GetMaxIdNumberByCategoryAsync(prefix);
        var productId = $"{prefix}-{maxNumber + 1}";
        
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            CategoryId = Guid.Parse(categoryId),
            ColorId = Guid.Parse(colorId),
            PatternId = Guid.Parse(patternId),
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
        string? categoryId,
        string? colorId,
        string? patternId,
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

        if (!string.IsNullOrWhiteSpace(categoryId))
            product.CategoryId = Guid.Parse(categoryId);
        
        if (!string.IsNullOrWhiteSpace(colorId))
            product.ColorId = Guid.Parse(colorId);
            
        if (!string.IsNullOrWhiteSpace(patternId))
            product.PatternId = Guid.Parse(patternId);

        if (!string.IsNullOrWhiteSpace(sizeType))
                product.SizeType = sizeType;

        if (salePrice.HasValue)
            product.SalePrice = salePrice.Value;
        
        if (importPrice.HasValue)
            product.ImportPrice = importPrice.Value;

        await _productsRepository.UpdateProduct(product);

        var updatedProduct = await _productsRepository.GetProductById(product.Id);

        var updatedQuantities = updatedProduct!.ProductQuantities.ToList();

        if (quantities is not null)
        {
            await _productQuantitiesRepository.DeleteProductQuantitiesByProductId(updatedProduct.Id);

            updatedQuantities = new List<ProductQuantity>();

            foreach (var quantity in quantities)
            {
                var productQuantity = new ProductQuantity
                {
                    Id = Guid.NewGuid(),
                    ProductId = updatedProduct.Id,
                    Size = quantity.Size,
                    Quantities = quantity.Quantities
                };

                await _productQuantitiesRepository.AddProductQuantities(productQuantity);
                updatedQuantities.Add(productQuantity);
            }
        }

        var imageUrl = "";
        if (!string.IsNullOrEmpty(updatedProduct.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(updatedProduct.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
        }

        var modelImageUrl = await GetModelImageUrlAsync(updatedProduct.ModelImageKey);

        return Result<ProductDto>.Success(new ProductDto(
            updatedProduct.Id,
            updatedProduct.ProductId,
            updatedProduct.ProductName,
            updatedProduct.Category.CategoryName,
            updatedProduct.Color.ColorName,
            updatedProduct.Pattern?.PatternName ?? "",
            updatedProduct.SizeType,
            updatedQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            updatedProduct.CreatedBy,
            updatedProduct.CreatedAt,
            updatedProduct.Status,
            imageUrl,
            updatedProduct.VectorId,
            updatedProduct.SalePrice,
            updatedProduct.ImportPrice,
            modelImageUrl
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
                product.Category.CategoryName,
                product.Color.ColorName,
                product.Pattern.PatternName,
                product.SizeType,
                product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
                product.CreatedBy,
                product.CreatedAt,
                product.Status,
                imageUrl,
                product.VectorId,
                product.SalePrice,
                product.ImportPrice,
                await GetModelImageUrlAsync(product.ModelImageKey)
            ));
        }

        return Result<PaginatedResult<ProductDto>>.Success(
            new PaginatedResult<ProductDto>(productDtos, total));
    }

    public async Task<Result<ProductWithQuantityChangesDto>> OwnerUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? colorId,
        string? patternId,
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

        if (!string.IsNullOrWhiteSpace(colorId))
            product.ColorId = Guid.Parse(colorId);
        
        if (!string.IsNullOrWhiteSpace(patternId))
            product.PatternId = Guid.Parse(patternId);

        if (!string.IsNullOrWhiteSpace(sizeType))
                product.SizeType = sizeType;

        if (salePrice.HasValue)
            product.SalePrice = salePrice.Value;

        if (importPrice.HasValue)
            product.ImportPrice = importPrice.Value;

        await _productsRepository.UpdateProduct(product);

        var updatedProduct = await _productsRepository.GetProductById(product.Id);

        if (updatedProduct == null)
        {
            return Result<ProductWithQuantityChangesDto>.Failure(new Error("NotFound", "Updated product not found."));
        }

        List<ProductQuantity> newQuantity = new();
        List<ProductsOrdersDetailQuantityChange> newQuantityChange = new();

        if (updatedProduct.Status == ProductStatus.Approved && newQuantities != null)
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
                    ProductId = updatedProduct.Id,
                    ProductsOrderId = Guid.Parse(productsOrderId),
                };

                await _productsOrdersDetailsRepository.CreateProductsOrdersDetails(detail);
            }

            var allSizes = updatedProduct.ProductQuantities.Select(q => q.Size).Union(newQuantities.Select(q => q.Size));

            foreach (var size in allSizes)
            {
                var currentQuantity = updatedProduct.ProductQuantities.FirstOrDefault(q => q.Size == size);
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

        if (updatedProduct.Status == ProductStatus.Pending && newQuantities != null)
        {
            await _productQuantitiesRepository.DeleteProductQuantitiesByProductId(updatedProduct.Id);

            foreach (var quantity in newQuantities)
            {
                var productQuantity = new ProductQuantity
                {
                    Id = Guid.NewGuid(),
                    ProductId = updatedProduct.Id,
                    Size = quantity.Size,
                    Quantities = quantity.Quantities
                };

                await _productQuantitiesRepository.AddProductQuantities(productQuantity);

                newQuantity.Add(productQuantity);
            }
        }

        var imageUrl = "";
        if (!string.IsNullOrEmpty(updatedProduct.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(updatedProduct.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
        }

        var productDto = new ProductDto(
            updatedProduct.Id,
            updatedProduct.ProductId,
            updatedProduct.ProductName,
            updatedProduct.Category.CategoryName,
            updatedProduct.Color.ColorName,
            updatedProduct.Pattern.PatternName,
            updatedProduct.SizeType,
            newQuantity.Count > 0 ? newQuantity.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList() : updatedProduct.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            updatedProduct.CreatedBy,
            updatedProduct.CreatedAt,
            updatedProduct.Status,
            imageUrl,
            updatedProduct.VectorId,
            updatedProduct.SalePrice,
            updatedProduct.ImportPrice
        );

        var quantityChanges = newQuantityChange.Select(c => new ProductQuantityChangeDto(c.Size, c.OldQuantity, c.NewQuantity)).ToList();

        return Result<ProductWithQuantityChangesDto>.Success(new ProductWithQuantityChangesDto(productDto, quantityChanges));
    }

    public async Task<Result<ProductWithQuantityChangesDto>> EmployeeUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? colorId,
        string? patternId,
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

        if (!string.IsNullOrWhiteSpace(colorId))
            product.ColorId = Guid.Parse(colorId);
            
        if (!string.IsNullOrWhiteSpace(patternId))
            product.PatternId = Guid.Parse(patternId);

        if (!string.IsNullOrWhiteSpace(sizeType))
                product.SizeType = sizeType;

        await _productsRepository.UpdateProduct(product);

        var updatedProduct = await _productsRepository.GetProductById(product.Id);

        if (updatedProduct == null)
        {
            return Result<ProductWithQuantityChangesDto>.Failure(new Error("NotFound", "Updated product not found."));
        }

        List<ProductQuantity> newQuantity = new();
        List<ProductsOrdersDetailQuantityChange> newQuantityChange = new();

        if (updatedProduct.Status == ProductStatus.Approved && newQuantities != null)
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
                    ProductId = updatedProduct.Id,
                    ProductsOrderId = Guid.Parse(productsOrderId),
                };

                await _productsOrdersDetailsRepository.CreateProductsOrdersDetails(detail);
            }

            var allSizes = updatedProduct.ProductQuantities.Select(q => q.Size).Union(newQuantities.Select(q => q.Size));

            foreach (var size in allSizes)
            {
                var currentQuantity = updatedProduct.ProductQuantities.FirstOrDefault(q => q.Size == size);
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

        if (updatedProduct.Status == ProductStatus.Pending && newQuantities != null)
        {
            await _productQuantitiesRepository.DeleteProductQuantitiesByProductId(updatedProduct.Id);

            foreach (var quantity in newQuantities)
            {
                var productQuantity = new ProductQuantity
                {
                    Id = Guid.NewGuid(),
                    ProductId = updatedProduct.Id,
                    Size = quantity.Size,
                    Quantities = quantity.Quantities
                };

                await _productQuantitiesRepository.AddProductQuantities(productQuantity);

                newQuantity.Add(productQuantity);
            }
        }

        var imageUrl = "";
        if (!string.IsNullOrEmpty(updatedProduct.ImageKey))
        {
            var imageResult = await _fileStorageService.GetImageUrlAsync(updatedProduct.ImageKey);
            imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
        }

        var productDto = new ProductDto(
            updatedProduct.Id,
            updatedProduct.ProductId,
            updatedProduct.ProductName,
            updatedProduct.Category.CategoryName,
            updatedProduct.Color.ColorName,
            updatedProduct.Pattern.PatternName,
            updatedProduct.SizeType,
            newQuantity.Count > 0 ? newQuantity.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList() : updatedProduct.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            updatedProduct.CreatedBy,
            updatedProduct.CreatedAt,
            updatedProduct.Status,
            imageUrl,
            updatedProduct.VectorId,
            updatedProduct.SalePrice,
            updatedProduct.ImportPrice
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

    public async Task<Result<List<ProductDto>>> FetchTop5LowStockProducts()
    {
        var products = await _productsRepository.FetchTop5LowStockProducts();

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
                product.Category.CategoryName,
                product.Color.ColorName,
                product.Pattern.PatternName,
                product.SizeType,
                product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
                product.CreatedBy,
                product.CreatedAt,
                product.Status,
                imageUrl,
                product.VectorId,
                product.SalePrice,
                product.ImportPrice,
                await GetModelImageUrlAsync(product.ModelImageKey)
            ));
        }

        return Result<List<ProductDto>>.Success(productDtos);
    }

    private async Task<string> GetModelImageUrlAsync(string modelImageKey)
    {
        if (string.IsNullOrWhiteSpace(modelImageKey))
            return string.Empty;

        var result = await _fileStorageService.GetImageUrlAsync(modelImageKey);
        return result.IsSuccess ? result.Value : string.Empty;
    }

    public async Task<Result> UpdateProductModelImageKey(string productId, string modelImageKey)
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(productId));

        if (product == null)
            return Result.Failure(new Error("ProductNotFound", "Product not found."));

        product.ModelImageKey = modelImageKey;

        await _productsRepository.UpdateProduct(product);

        return Result.Success();
    }

    public async Task<Result<string>> GenerateModelImage(string productId)
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(productId));

        if (product == null)
            return Result<string>.Failure(new Error("ProductNotFound", "Product not found."));

        if (string.IsNullOrEmpty(product.ImageKey))
            return Result<string>.Failure(new Error("NoProductImage", "Product does not have an image."));

        var imageUrlResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
        if (imageUrlResult.IsFailure)
            return Result<string>.Failure(new Error("ImageRetrievalFailed", "Failed to retrieve product image."));

        using var client = new HttpClient();
        var imageBytes = await client.GetByteArrayAsync(imageUrlResult.Value);
        var imageBase64 = Convert.ToBase64String(imageBytes);
        var imageDataUri = $"data:image/jpeg;base64,{imageBase64}";

        try
        {
            var modelImageBase64 = await _modelPromptProvider.GenerateModelImageAsync(imageDataUri, product.ProductName);

            var stream = new MemoryStream(Convert.FromBase64String(modelImageBase64.Split(',')[1]));
            var uploadResult = await _fileStorageService.UploadImageAsync(
                "product-models",
                productId,
                stream,
                "image/jpeg",
                ".jpg"
            );

            if (uploadResult.IsFailure)
                return Result<string>.Failure(new Error("UploadFailed", "Failed to upload generated model image."));

            await UpdateProductModelImageKey(productId, uploadResult.Value);

            return Result<string>.Success(uploadResult.Value);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error("GenerationFailed", $"Failed to generate model image: {ex.Message}"));
        }
    }

    public async Task<Result<List<CategoryDto>>> FetchAllCategories()
    {
        var categories = await _categoryRepository.FetchCategories();

        var categoryDtos = categories.Select(c => new CategoryDto(c.Id, c.CategoryName)).ToList();

        return Result<List<CategoryDto>>.Success(categoryDtos);
    }

    public async Task<Result<List<ColorDto>>> FetchAllColors()
    {
        var colors = await _colorRepository.FetchColors();

        var colorDtos = colors.Select(c => new ColorDto(c.Id, c.ColorName)).ToList();

        return Result<List<ColorDto>>.Success(colorDtos);
    }

    public async Task<Result<List<PatternDto>>> FetchAllPatterns()
    {
        var patterns = await _patternRepository.FetchPatterns();

        var patternDtos = patterns.Select(p => new PatternDto(p.Id, p.PatternName)).ToList();

        return Result<List<PatternDto>>.Success(patternDtos);
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