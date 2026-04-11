using Capstone.Application.Common;

namespace Capstone.Application.Services.SaleOrderDetails;

public interface ISaleOrderDetailsService
{
    Task<Result> CreateSaleOrderDetail(
        string SaleOrderId,
        string ProductId,
        string SelectedSize,
        int Quantity,
        double Discount
    );
}