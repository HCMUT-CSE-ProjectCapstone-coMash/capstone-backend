using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.SaleOrderDetails;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.SaleOrders;

public class SaleOrdersService : ISaleOrdersService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly ISaleOrdersRepository _saleOrdersRepository;

    public SaleOrdersService(IDateTimeProvider dateTimeProvider, ISaleOrdersRepository saleOrdersRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _saleOrdersRepository = saleOrdersRepository;
    }

    public async Task<Result<string>> CreateSaleOrder(
        string CustomerId,
        string CreatedBy,
        string PaymentMethod,
        double DebitMoney,
        double Discount
    )
    {
        var newSaleOrder = new SaleOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = string.IsNullOrEmpty(CustomerId) ? null : Guid.Parse(CustomerId),
            CreatedBy = Guid.Parse(CreatedBy),
            PaymentMethod = PaymentMethod,
            DebitMoney = DebitMoney,
            CreatedAt = _dateTimeProvider.UtcNow,
            Discount = Discount
        };

        await _saleOrdersRepository.CreateSaleOrder(newSaleOrder);

        return Result<string>.Success(newSaleOrder.Id.ToString());
    }

    public async Task<Result<SaleOrderDto>> GetSaleOrderById(string saleOrderId)
    {
        var saleOrder = await _saleOrdersRepository.GetSaleOrderWithDetails(Guid.Parse(saleOrderId));

        if (saleOrder == null)
            return Result<SaleOrderDto>.Failure(new Error("SaleOrderNotFound", "Sale order not found"));

        var details = saleOrder.SaleOrderDetails.Select(d => new SaleOrderDetailDto
        {
            Id = d.Id,
            ProductId = d.ProductId,
            ProductName = d.Product.ProductName,
            SelectedSize = d.SelectedSize,
            Quantity = d.Quantity,
            UnitPrice = d.Product.SalePrice,
            Discount = d.Discount,
            SubTotal = d.Quantity * d.Product.SalePrice * (1 - d.Discount / 100)
        }).ToList();

        var totalBeforeDiscount = details.Sum(d => d.SubTotal);

        var dto = new SaleOrderDto
        {
            Id = saleOrder.Id,
            CustomerId = saleOrder.CustomerId,
            CustomerName = saleOrder.Customer?.CustomerName,
            CreatedBy = saleOrder.CreatedBy,
            CreatedByName = saleOrder.User.FullName,
            PaymentMethod = saleOrder.PaymentMethod,
            DebitMoney = saleOrder.DebitMoney,
            CreatedAt = saleOrder.CreatedAt,
            Discount = saleOrder.Discount,
            Details = details,
            TotalPrice = totalBeforeDiscount * (1 - saleOrder.Discount / 100)
        };

        return Result<SaleOrderDto>.Success(dto);
    }
}