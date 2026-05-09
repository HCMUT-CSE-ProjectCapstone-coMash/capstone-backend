using Capstone.Application.Common;

namespace Capstone.Application.Services.SaleOrders;

public interface ISaleOrdersService
{
    Task<Result<string>> CreateSaleOrder(
        string CustomerId,
        string CreatedBy,
        string PaymentMethod,
        double DebitMoney
    );

    Task<Result> UpdateTotalPriceAndTotalProfit(string saleOrderId, string orderPromotionId);

    Task<Result<SaleOrderDto>> GetSaleOrderById(string saleOrderId);

    Task<Result<PaginatedResult<SaleOrderDto>>> FetchAllSaleOrders(int page, int pageSize, string? period = null, string? search = null);

    Task<Result<PaginatedResult<SaleOrderDto>>> FetchAllSaleOrdersByEmployeeId(string employeeId, int page, int pageSize, string? search = null);

    Task<Result<PaginatedResult<SaleOrderDto>>> FetchAllSaleOrdersByCustomerId(string customerId, int page, int pageSize, string? search = null);

    Task<Result<List<SaleOrderDto>>> GetAllSaleOrdersWithDebt(string customerId);

    Task<Result<List<SaleOrderDto>>> PayDebt(string customerId, double paymentAmount);

    Task<Result<IncomeStatsDto>> GetIncomeStats(string period);

    Task<Result<PersonalIncomeStatsDto>> GetPersonalIncomeStats(string employeeId, string period);

    Task<Result<TopCustomerStatsDto>> GetTopCustomersSpendingStats(int limit);

    Task<Result<DashboardStatsDto>> GetDashboardStats();
}