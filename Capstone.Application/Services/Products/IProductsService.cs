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
    
    Task<Result<ProductDto>> UpdateProduct(
        Guid productId,
        string? productID,
        string? productName,
        string? category,
        string? color,
        string? pattern,
        string? sizeType,
        List<ProductQuantityDto>? quantities
    );
}