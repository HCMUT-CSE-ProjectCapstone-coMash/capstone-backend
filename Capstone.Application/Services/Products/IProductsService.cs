using Capstone.Application.Common;

namespace Capstone.Application.Services.Products;

public interface IProductsService
{
    Task<Result<ProductDto>> CreateProduct(
        string productID,
        string ProductName,
        string category,
        string color,
        string pattern,
        string sizeType,
        List<ProductQuantityDto> productQuantities,
        string createdBy
    );

    Task<Result<List<ProductDto>>> GetPendingProductsByUserId(Guid userId);
}