using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
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

    public async Task<Result<SaleOrderDto>> CreateSaleOrder(
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

        return Result<SaleOrderDto>.Success(new SaleOrderDto
        {
            Id = newSaleOrder.Id,
            CustomerId = newSaleOrder.CustomerId,
            CreatedBy = newSaleOrder.CreatedBy,
            PaymentMethod = newSaleOrder.PaymentMethod,
            DebitMoney = newSaleOrder.DebitMoney,
            CreatedAt = newSaleOrder.CreatedAt,
            Discount = newSaleOrder.Discount
        });
    }
}