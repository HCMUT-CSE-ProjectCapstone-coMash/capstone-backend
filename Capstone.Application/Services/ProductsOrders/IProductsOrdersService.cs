using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductsOrders;

public interface IProductsOrdersService
{
    Task<Result<ProductsOrdersDto>> FetchOrCreateProductsOrders(string createdBy);
    Task<Result<List<ProductsOrdersListDto>>> GetAllProductsOrders();
    Task<Result<string>> DeleteProductFromProductsOrders(string orderId, string productId);
    Task<Result<string>> PatchProductsOrders(string orderId, string? orderName, string? OrderDescription, string? OrderStatus);
}