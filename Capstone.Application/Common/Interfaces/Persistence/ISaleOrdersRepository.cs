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
    Task<PersonalIncomeStatsDto> GetPersonalIncomeStats(Guid employeeId, string period);
    Task<TopCustomerStatsDto> GetTopCustomersSpendingStats(int limit);
    Task<DashboardStatsDto> GetDashboardStats();
}

// -- Income stats --
public class IncomeGroupDto
{
    public string Key { get; set; } = "";
    public double Total { get; set; }
    public double Profit { get; set; }
}

public class IncomeStatsDto
{
    public string Period { get; set; } = "";
    public double Total { get; set; }
    public double TotalProfit { get; set; }
    public List<IncomeGroupDto> Groups { get; set; } = [];
}

// -- Personal income stats --
public class PersonalIncomeGroupDto
{
    public string Key { get; set; } = "";
    public double Total { get; set; }
}

public class PersonalIncomeStatsDto
{
    public string Period { get; set; } = "";
    public double Total { get; set; }
    public List<PersonalIncomeGroupDto> Groups { get; set; } = [];
} 

// -- 
public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Total { get; set; }
}

public class TopCustomerStatsDto
{
    public List<TopCustomerDto> Customers { get; set; } = [];
    public double WalkInTotal { get; set; }
    public double GrandTotal { get; set; }
}

public class DashboardStatsDto
{
    public double TotalSaleToday { get; set; }
    public double ProfitToday { get; set; }
    public int TotalOrderToday { get; set; }
    public double TotalSaleYesterday { get; set; }
    public double ProfitYesterday { get; set; }
    public int TotalOrderYesterday { get; set; }
}