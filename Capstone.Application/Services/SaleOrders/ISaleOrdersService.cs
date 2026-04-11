using Capstone.Application.Common;

namespace Capstone.Application.Services.SaleOrders;

public interface ISaleOrdersService
{
    Task<Result<string>> CreateSaleOrder(
        string CustomerId,
        string CreatedBy,
        string PaymentMethod,
        double DebitMoney,
        double Discount
    );

    Task<Result<SaleOrderDto>> GetSaleOrderById(string saleOrderId);
}