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

    Task<Result<ProductDto>> PatchProductInProductsOrders(
        string id,
        string? productId,
        string? productName,
        string? category,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? quantities
    );

    Task<Result<List<ProductDto>>> FetchApprovedProductByName(string productName);

    Task<Result<string>> CreateProductIdByCategory(string category);

    Task<Result<ProductWithQuantityChangesDto>> CreateDetailForApprovedProduct(
        string productId,
        string productsOrderId,
        List<ProductQuantityDto> productQuantities
    );

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

    Task<Result<(List<ProductDto> Items, int Total)>> FetchAllProducts(int currentPage, int pageSize);
}