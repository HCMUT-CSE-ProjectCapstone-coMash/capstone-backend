using Capstone.Application.Common;

namespace Capstone.Application.Services.SaleOrders;

public interface ISaleOrdersService
{
    Task<Result<SaleOrderDto>> CreateSaleOrder(
        string CustomerId,
        string CreatedBy,
        string PaymentMethod,
        double DebitMoney,
        double Discount
    );
}