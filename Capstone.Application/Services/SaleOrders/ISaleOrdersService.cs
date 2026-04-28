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

    Task<Result<PaginatedResult<SaleOrderDto>>> FetchAllSaleOrders(int page, int pageSize, string? search = null);

    Task<Result<PaginatedResult<SaleOrderDto>>> FetchAllSaleOrdersByEmployeeId(string employeeId, int page, int pageSize, string? search = null);
}