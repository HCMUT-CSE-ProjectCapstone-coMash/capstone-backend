using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductsOrders;

public interface IProductsOrdersService
{
    Task<Result<ProductsOrdersDto>> FetchOrCreateProductsOrders(string createdBy);
    Task<Result<PaginatedResult<ProductsOrdersListDto>>> GetAllProductsOrdersExcludingPending(int currentPage, int pageSize, string? search = null);
    Task<Result<string>> DeleteProductFromProductsOrders(string orderId, string productId);
    Task<Result<string>> PatchProductsOrders(string orderId, string? orderName, string? OrderDescription, string? OrderStatus);
    Task<Result<ProductsOrdersDto>> GetProductsOrderById(string orderId);
    Task<Result<string>> ApproveProductsOrder(string orderId);
    Task<Result<string>> DeleteProductsOrder(string orderId);
}