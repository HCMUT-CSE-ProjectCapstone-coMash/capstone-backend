using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsOrdersRepository
{
    Task<ProductsOrder?> GetProductsOrdersByCreatedByAndStatus(Guid createdBy, string status);
    Task CreateProductsOrders(ProductsOrder productsOrder);
    Task<ProductsOrder?> GetProductsOrdersByOrderId(Guid orderId);
    Task PatchProductsOrders(ProductsOrder productsOrder);
}