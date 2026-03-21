using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductsOrders;

public interface IProductsOrdersService
{
    Task<Result<ProductsOrdersDto>> FetchOrCreateProductsOrders(string createdBy);
    Task<Result<string>> DeleteProductFromProductsOrders(string orderId, string productId);
}