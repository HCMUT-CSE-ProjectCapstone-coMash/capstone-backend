using Capstone.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Capstone.Application.Services.Products;

public interface IProductsService
{
    Task<Result<ProductDto>> CreateProduct(
        string productId,
        string ProductName,
        string category,
        string color,
        string? pattern,
        string sizeType,
        List<ProductQuantityDto> productQuantities,
        string createdBy,
        IFormFile? image,
        string orderID
    );

    Task<Result<ProductDto>> SearchProductSimilar(string ImageBase64);

    Task<Result<AnalyzeProductDto>> AnalyzeImage(string ImageBase64);

    Task<Result<List<ProductWithOrderStatusDto>>> FetchApprovedProductByName(string productName);

    Task<Result<string>> CreateProductIdByCategory(string category);

    Task<Result<ProductDto>> OwnerCreateProduct(
        string productId,
        string productName,
        string category,
        string color,
        string? pattern,
        string sizeType,
        List<ProductQuantityDto> productQuantities,
        string createdBy,
        IFormFile? image,
        decimal salePrice,
        decimal importPrice
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
        decimal? salePrice,
        decimal? importPrice
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
        decimal? salePrice,
        decimal? importPrice
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