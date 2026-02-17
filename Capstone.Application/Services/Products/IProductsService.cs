using Capstone.Application.Common;

namespace Capstone.Application.Services.Products;

public interface IProductsService
{
    Task<Result<CreateProductResult>> CreateProduct(
        string productID,
        string ProductName,
        string category,
        string color,
        string pattern,
        string sizeType,
        List<ProductQuantity> productQuantities
    );
}