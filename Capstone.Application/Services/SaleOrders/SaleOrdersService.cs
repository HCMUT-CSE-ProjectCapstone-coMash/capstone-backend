using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.SaleOrderDetails;
using Capstone.Domain.Common;
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
        double DebitMoney
    )
    {
        var maxNumber = await _saleOrdersRepository.GetMaxIdNumber();
        var saleOrderId = $"HD-{maxNumber + 1}";

        var newDebitMoney= PaymentMethodStatus.Debit == PaymentMethod
            ? DebitMoney
            : 0;

        var newSaleOrder = new SaleOrder
        {
            Id = Guid.NewGuid(),
            SaleOrderId = saleOrderId,
            CustomerId = string.IsNullOrEmpty(CustomerId) ? null : Guid.Parse(CustomerId),
            CreatedBy = Guid.Parse(CreatedBy),
            PaymentMethod = PaymentMethod,
            DebitMoney = newDebitMoney,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _saleOrdersRepository.CreateSaleOrder(newSaleOrder);

        return Result<string>.Success(newSaleOrder.Id.ToString());
    }

    public async Task<Result> UpdateTotalPriceAndTotalProfit(string saleOrderId)
    {
        var saleOrder = await _saleOrdersRepository.GetSaleOrderWithDetails(Guid.Parse(saleOrderId));

        if (saleOrder == null)
            return Result.Failure(new Error("SaleOrderNotFound", "Sale order not found"));

        var totalPrice = saleOrder.SaleOrderDetails.Sum(d => d.SubTotal);
        var totalProfit = saleOrder.SaleOrderDetails.Sum(d => d.Profit);

        saleOrder.TotalPrice = totalPrice;
        saleOrder.TotalProfit = totalProfit;
        await _saleOrdersRepository.UpdateSaleOrder(saleOrder);

        return Result.Success();
    }

    public async Task<Result<PaginatedResult<SaleOrderDto>>> FetchAllSaleOrders(int page, int pageSize, string? search = null)
    {
        var saleOrders = await _saleOrdersRepository.FetchAllSaleOrders(page, pageSize, search);

        var saleOrderDtos = saleOrders.Items.Select(so => new SaleOrderDto
        {
            Id = so.Id,
            SaleOrderId = so.SaleOrderId,
            CustomerId = so.CustomerId,
            CustomerName = so.Customer?.CustomerName,
            CreatedBy = so.CreatedBy,
            CreatedByName = so.User.FullName,
            PaymentMethod = so.PaymentMethod,
            DebitMoney = so.DebitMoney,
            CreatedAt = so.CreatedAt,
            TotalPrice = so.TotalPrice,
            TotalProfit = so.TotalProfit,
            Details = new List<SaleOrderDetailDto>()
        }).ToList();

        return Result<PaginatedResult<SaleOrderDto>>.Success(new PaginatedResult<SaleOrderDto>(saleOrderDtos, saleOrders.Total));
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
            UnitPrice = d.UnitPrice,
            Discount = d.Discount,
            SubTotal = d.SubTotal
        }).ToList();

        var dto = new SaleOrderDto
        {
            Id = saleOrder.Id,
            SaleOrderId = saleOrder.SaleOrderId,
            CustomerId = saleOrder.CustomerId,
            CustomerName = saleOrder.Customer?.CustomerName,
            CreatedBy = saleOrder.CreatedBy,
            CreatedByName = saleOrder.User.FullName,
            PaymentMethod = saleOrder.PaymentMethod,
            DebitMoney = saleOrder.DebitMoney,
            CreatedAt = saleOrder.CreatedAt,
            Details = details,
            TotalPrice = saleOrder.TotalPrice
        };

        return Result<SaleOrderDto>.Success(dto);
    }
}