using Capstone.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Capstone.Application.Services.Products;

public interface IProductsService
{
    Task<Result<string>> CreateProduct(
        string ProductName,
        string categoryId,
        string colorId,
        string patternId,
        string sizeType,
        string createdBy
    );

    Task<Result> UpdateProductImageKey(string productId, string imageKey, float[] vector);

    Task<Result<ProductDto>> FetchProductById(string id);

    Task<Result<ProductDto>> FetchProductByProductId(string productId);

    Task<Result<AnalyzeProductDto>> AnalyzeImage(string ImageBase64);

    Task<Result<List<ProductWithOrderStatusDto>>> FetchApprovedProductByName(string productName);

    Task<Result<string>> CreateProductIdByCategoryId(string categoryId);

    Task<Result<string>> OwnerCreateProduct(
        string productName,
        string categoryId,
        string colorId,
        string patternId,
        string sizeType,
        string createdBy,
        double salePrice,
        double importPrice
    );

    Task<Result<ProductDto>> OwnerPatchProduct(
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
    );

    Task<Result<PaginatedResult<ProductDto>>> FetchAllProducts(int currentPage, int pageSize, string? category = null, string? search = null);

    Task<Result<ProductWithQuantityChangesDto>> OwnerUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? colorId,
        string? patternId,
        string? sizeType,
        List<ProductQuantityDto>? newQuantities,
        double? salePrice,
        double? importPrice
    );

    Task<Result<ProductWithQuantityChangesDto>> EmployeeUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? colorId,
        string? patternId,
        string? sizeType,
        List<ProductQuantityDto>? newQuantities
    );

    Task<Result<string>> DeleteProduct(string id);

    Task<Result<List<ProductDto>>> FetchTop5LowStockProducts();

    Task<Result> UpdateProductModelImageKey(string productId, string modelImageKey);

    Task<Result<string>> GenerateModelImage(string productId);

    Task<Result<List<CategoryDto>>> FetchAllCategories();
    
     Task<Result<List<ColorDto>>> FetchAllColors();

     Task<Result<List<PatternDto>>> FetchAllPatterns();
}