using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductsOrdersDetailService;

public interface IProductsOrdersDetailService
{
    Task<Result> CreateProductsOrdersDetail(string orderId, string productId);
}