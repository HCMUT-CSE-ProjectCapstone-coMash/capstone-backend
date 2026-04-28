using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.ComboPromotionsService;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.OrderPromotionsService;
using Capstone.Application.Services.ProductPromotionsService;
using Capstone.Application.Services.Products;
using Capstone.Application.Services.SaleOrderDetails;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.SaleOrders;

public class SaleOrdersService : ISaleOrdersService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileStorageService _fileStorageService;

    private readonly ISaleOrdersRepository _saleOrdersRepository;
    private readonly IOrderPromotionsRepository _orderPromotionsRepository;
    private readonly ISaleOrderDetailsRepository _saleOrderDetailsRepository;

    public SaleOrdersService(
        IDateTimeProvider dateTimeProvider,
        IFileStorageService fileStorageService,
        ISaleOrdersRepository saleOrdersRepository,
        IOrderPromotionsRepository orderPromotionsRepository,
        ISaleOrderDetailsRepository saleOrderDetailsRepository
    )
    {
        _dateTimeProvider = dateTimeProvider;
        _fileStorageService = fileStorageService;
        _saleOrdersRepository = saleOrdersRepository;
        _orderPromotionsRepository = orderPromotionsRepository;
        _saleOrderDetailsRepository = saleOrderDetailsRepository;
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

        var newDebitMoney = PaymentMethodStatus.Debit == PaymentMethod
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

    public async Task<Result> UpdateTotalPriceAndTotalProfit(string saleOrderId, string orderPromotionId)
    {
        var saleOrder = await _saleOrdersRepository.GetSaleOrderWithDetails(Guid.Parse(saleOrderId));

        if (saleOrder == null)
            return Result.Failure(new Error("SaleOrderNotFound", "Sale order not found"));

        var grossTotal = saleOrder.SaleOrderDetails.Sum(d => d.SubTotal);

        double orderDiscountAmount = 0;
        if (!string.IsNullOrEmpty(orderPromotionId))
        {
            var orderPromotionResult = await _orderPromotionsRepository.GetOrderPromotionByOrderPromotionId(Guid.Parse(orderPromotionId));

            if (orderPromotionResult == null)
                return Result.Failure(new Error("OrderPromotionNotFound", "Order promotion not found"));

            if (orderPromotionResult.DiscountType == DiscountType.Percent)
            {
                orderDiscountAmount = Math.Min(
                    grossTotal * (orderPromotionResult.DiscountValue / 100),
                    orderPromotionResult.MaxDiscount > 0 ? orderPromotionResult.MaxDiscount : double.MaxValue
                );
            }
            else if (orderPromotionResult.DiscountType == DiscountType.Fixed)
            {
                orderDiscountAmount = orderPromotionResult.DiscountValue;
            }

            saleOrder.AppliedOrderPromotionId = Guid.Parse(orderPromotionId);
        }

        var finalTotal = grossTotal - orderDiscountAmount;

        foreach (var detail in saleOrder.SaleOrderDetails)
        {
            var proportion = grossTotal > 0 ? detail.SubTotal / grossTotal : 0;
            var lineDiscount = Math.Round(orderDiscountAmount * proportion, 2);
            var lineEffectiveSubTotal = detail.SubTotal - lineDiscount;

            detail.Profit = Math.Round(lineEffectiveSubTotal - (detail.Product.ImportPrice * detail.Quantity));

            await _saleOrderDetailsRepository.UpdateSaleOrderDetail(detail);
        }

        saleOrder.TotalPrice = finalTotal;
        saleOrder.TotalProfit = saleOrder.SaleOrderDetails.Sum(d => d.Profit);

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
            CustomerPhone = so.Customer?.CustomerPhoneNumber,
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

        var details = new List<SaleOrderDetailDto>();

        foreach (var detail in saleOrder.SaleOrderDetails)
        {
            var imageUrl = "";
            if (!string.IsNullOrEmpty(detail.Product.ImageKey))
            {
                var imageResult = await _fileStorageService.GetImageUrlAsync(detail.Product.ImageKey);
                imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
            }

            var productPromotionDto = detail.ProductPromotion != null
                ? new ProductPromotionDto
                {
                    Id = detail.ProductPromotion.Id,
                    DiscountType = detail.ProductPromotion.DiscountType,
                    DiscountValue = detail.ProductPromotion.DiscountValue,
                    PromotionId = detail.ProductPromotion.Promotion.Id,
                    PromotionName = detail.ProductPromotion.Promotion.PromotionName
                }
                : null;

            var comboPromotionDto = detail.ComboPromotion != null
                ? new ComboPromotionDto
                {
                    Id = detail.ComboPromotion.Id,
                    ComboName = detail.ComboPromotion.ComboName,
                    ComboPrice = detail.ComboPromotion.ComboPrice,
                    PromotionId = detail.ComboPromotion.Promotion.Id,
                    ComboItems = detail.ComboPromotion.ComboPromotionDetails.Select(cpd => new ComboPromotionDetailDto
                    {
                        Id = cpd.Id,
                        ProductId = cpd.Product.Id,
                        Quantity = cpd.Quantity
                    }).ToList()
                }
                : null;

            var detailDto = new SaleOrderDetailDto
            {
                Id = detail.Id,
                ProductId = detail.ProductId,
                ProductName = detail.Product.ProductName,
                ImageUrl = imageUrl,
                SelectedSize = detail.SelectedSize,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                Discount = detail.Discount,
                SubTotal = detail.SubTotal,
                Profit = detail.Profit,
                ProductPromotion = productPromotionDto,
                ComboPromotion = comboPromotionDto
            };

            details.Add(detailDto);
        }

        var orderPromotion = saleOrder.AppliedOrderPromotion != null
            ? new OrderPromotionDto
            {
                Id = saleOrder.AppliedOrderPromotion.Id,
                DiscountType = saleOrder.AppliedOrderPromotion.DiscountType,
                DiscountValue = saleOrder.AppliedOrderPromotion.DiscountValue,
                MaxDiscount = saleOrder.AppliedOrderPromotion.MaxDiscount,
                MinValue = saleOrder.AppliedOrderPromotion.MinValue,
            }
            : null;

        var originalTotalPrice = saleOrder.SaleOrderDetails.Sum(d => d.SubTotal);

        var dto = new SaleOrderDto
        {
            Id = saleOrder.Id,
            SaleOrderId = saleOrder.SaleOrderId,
            CustomerId = saleOrder.CustomerId,
            CustomerName = saleOrder.Customer?.CustomerName,
            CustomerPhone = saleOrder.Customer?.CustomerPhoneNumber,
            CreatedBy = saleOrder.CreatedBy,
            CreatedByName = saleOrder.User.FullName,
            PaymentMethod = saleOrder.PaymentMethod,
            DebitMoney = saleOrder.DebitMoney,
            CreatedAt = saleOrder.CreatedAt,
            Details = details,
            TotalPrice = saleOrder.TotalPrice,
            TotalProfit = saleOrder.TotalProfit,
            OriginalTotalPrice = originalTotalPrice,
            AppliedOrderPromotion = orderPromotion,
            AppliedOrderPromotionName = saleOrder.AppliedOrderPromotion != null ? saleOrder.AppliedOrderPromotion.Promotion.PromotionName : null
        };

        return Result<SaleOrderDto>.Success(dto);
    }

    public async Task<Result<PaginatedResult<SaleOrderDto>>> FetchAllSaleOrdersByEmployeeId(string employeeId, int page, int pageSize, string? search = null)
    {
        var saleOrders = await _saleOrdersRepository.FetchAllSaleOrdersByEmployeeId(Guid.Parse(employeeId), page, pageSize, search);

        var saleOrderDtos = saleOrders.Items.Select(so => new SaleOrderDto
        {
            Id = so.Id,
            SaleOrderId = so.SaleOrderId,
            CustomerId = so.CustomerId,
            CustomerName = so.Customer?.CustomerName,
            CustomerPhone = so.Customer?.CustomerPhoneNumber,
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
}