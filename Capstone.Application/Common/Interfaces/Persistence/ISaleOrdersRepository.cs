using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ISaleOrdersRepository
{
    Task CreateSaleOrder(SaleOrder saleOrder);
    Task UpdateSaleOrder(SaleOrder saleOrder);
    Task<SaleOrder?> GetSaleOrderWithDetails(Guid saleOrderId);
    Task<int> GetMaxIdNumber();
    Task<bool> ExistsByEmployeeId(Guid employeeId);
    Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrders(int page, int pageSize, string? period = null, string? search = null);
    Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrdersByEmployeeId(Guid employeeId, int page, int pageSize, string? search = null);
    Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrdersByCustomerId(Guid customerId, int page, int pageSize, string? search = null);
    Task<List<SaleOrder>> GetAllSaleOrdersWithDebt(Guid customerId);
    Task<IncomeStatsDto> GetIncomeStats(string period);
}

public class IncomeGroupDto
{
    public string Key { get; set; } = "";
    public double Total { get; set; }
}

public class IncomeStatsDto
{
    public string Period { get; set; } = "";
    public double Total { get; set; }
    public List<IncomeGroupDto> Groups { get; set; } = [];
} 