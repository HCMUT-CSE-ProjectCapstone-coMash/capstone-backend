using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.ComboPromotionsService;
using Capstone.Application.Services.FileStorageService;
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

    public SaleOrdersService(
        IDateTimeProvider dateTimeProvider,
        IFileStorageService fileStorageService,
        ISaleOrdersRepository saleOrdersRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _fileStorageService = fileStorageService;
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
            TotalPrice = saleOrder.TotalPrice,
            TotalProfit = saleOrder.TotalProfit
        };

        return Result<SaleOrderDto>.Success(dto);
    }
}