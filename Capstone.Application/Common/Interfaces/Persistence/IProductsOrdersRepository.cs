using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsOrdersRepository
{
    Task<ProductsOrder?> GetProductsOrdersByCreatedBy(Guid createdBy);
    Task<List<ProductsOrder>> GetProductsOrdersExcludingPending();
    Task CreateProductsOrders(ProductsOrder productsOrder);
    Task<ProductsOrder?> GetProductsOrdersByOrderId(Guid orderId);
    Task PatchProductsOrders(ProductsOrder productsOrder);
    Task<List<Guid>> GetProductIdsInPendingAndSendingOrders();
}