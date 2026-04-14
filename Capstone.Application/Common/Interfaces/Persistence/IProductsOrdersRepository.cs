using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsOrdersRepository
{
    Task<ProductsOrder?> GetProductsOrdersByCreatedBy(Guid createdBy);
    Task<(List<ProductsOrder> Items, int Total)> GetProductsOrdersExcludingPending(int currentPage, int pageSize, string? search = null);
    Task CreateProductsOrders(ProductsOrder productsOrder);
    Task<ProductsOrder?> GetProductsOrdersByOrderId(Guid orderId);
    Task PatchProductsOrders(ProductsOrder productsOrder);
    Task DeleteProductsOrder(ProductsOrder productsOrder);
    Task<List<Guid>> GetProductIdsInPendingAndSendingOrders();
    Task<bool> ExistsByEmployeeId(Guid employeeId);
}