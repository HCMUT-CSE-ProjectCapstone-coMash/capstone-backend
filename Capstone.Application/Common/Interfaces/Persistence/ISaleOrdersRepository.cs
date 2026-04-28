using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ISaleOrdersRepository
{
    Task CreateSaleOrder(SaleOrder saleOrder);
    Task UpdateSaleOrder(SaleOrder saleOrder);
    Task<SaleOrder?> GetSaleOrderWithDetails(Guid saleOrderId);
    Task<int> GetMaxIdNumber();
    Task<bool> ExistsByEmployeeId(Guid employeeId);
    Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrders(int page, int pageSize, string? search = null);
    Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrdersByEmployeeId(Guid employeeId, int page, int pageSize, string? search = null);
    Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrdersByCustomerId(Guid customerId, int page, int pageSize, string? search = null);
}