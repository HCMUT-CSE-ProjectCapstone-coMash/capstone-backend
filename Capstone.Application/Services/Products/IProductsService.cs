using Capstone.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Capstone.Application.Services.Products;

public interface IProductsService
{
    Task<Result<string>> CreateProduct(
        string ProductName,
        string category,
        string color,
        string pattern,
        string sizeType,
        string createdBy
    );

    Task<Result> UpdateProductImageKey(string productId, string imageKey);

    Task<Result<ProductDto>> FetchProductById(string id);

    Task<Result<ProductDto>> SearchProductSimilar(string ImageBase64);

    Task<Result<AnalyzeProductDto>> AnalyzeImage(string ImageBase64);

    Task<Result<List<ProductWithOrderStatusDto>>> FetchApprovedProductByName(string productName);

    Task<Result<string>> CreateProductIdByCategory(string category);

    Task<Result<string>> OwnerCreateProduct(
        string productName,
        string category,
        string color,
        string pattern,
        string sizeType,
        string createdBy,
        double salePrice,
        double importPrice
    );

    Task<Result<ProductDto>> OwnerPatchProduct(
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
    );

    Task<Result<PaginatedResult<ProductDto>>> FetchAllProducts(int currentPage, int pageSize, string? category = null, string? search = null);

    Task<Result<ProductWithQuantityChangesDto>> OwnerUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? newQuantities,
        double? salePrice,
        double? importPrice
    );

    Task<Result<ProductWithQuantityChangesDto>> EmployeeUpdateProductInProductsOrder(
        string id,
        string productsOrderId,
        string? productName,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? newQuantities
    );
}