using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.SaleOrderDetails;

public class SaleOrderDetailsService : ISaleOrderDetailsService
{
    private readonly ISaleOrderDetailsRepository _saleOrderDetailsRepository;
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IProductPromotionsRepository _productPromotionsRepository;
    private readonly IComboPromotionsRepository _comboPromotionsRepository;

    public SaleOrderDetailsService(
        ISaleOrderDetailsRepository saleOrderDetailsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IProductsRepository productsRepository,
        IProductPromotionsRepository productPromotionsRepository,
        IComboPromotionsRepository comboPromotionsRepository
    )
    {
        _saleOrderDetailsRepository = saleOrderDetailsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _productsRepository = productsRepository;
        _productPromotionsRepository = productPromotionsRepository;
        _comboPromotionsRepository = comboPromotionsRepository;
    }

    public async Task<Result> CreateSaleOrderDetailForProductPromotion(
        string SaleOrderId,
        string ProductId,
        string SelectedSize,
        int Quantity,
        double Discount,
        string PromotionId
    )
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(ProductId));

        if (product == null)
        {
            return Result.Failure(new Error("ProductNotFound", "Product not found"));
        }

        var UnitPrice = product.SalePrice;

        if (!string.IsNullOrEmpty(PromotionId))
        {
            var productPromotion = await _productPromotionsRepository.GetProductPromotionById(Guid.Parse(PromotionId));

            if (productPromotion == null)
            {
                return Result.Failure(new Error("ProductPromotionNotFound", "Product promotion not found"));
            }

            if (productPromotion.DiscountType == DiscountType.Percent)
            {
                UnitPrice = UnitPrice * (1 - productPromotion.DiscountValue / 100);
            }
            else if (productPromotion.DiscountType == DiscountType.Fixed)
            {
                UnitPrice = UnitPrice - productPromotion.DiscountValue;
            }
        }

        UnitPrice = UnitPrice * (1 - Discount / 100);
        var SubTotal = Quantity * UnitPrice;
        var profit = SubTotal - (Quantity * product.ImportPrice);

        var saleOrderDetail = new SaleOrderDetail
        {
            Id = Guid.NewGuid(),
            SaleOrderId = Guid.Parse(SaleOrderId),
            ProductId = Guid.Parse(ProductId),
            SelectedSize = SelectedSize,
            Quantity = Quantity,
            Discount = Discount,
            UnitPrice = UnitPrice,
            SubTotal = SubTotal,
            Profit = profit,
            ProductPromotionId = string.IsNullOrEmpty(PromotionId) ? null : Guid.Parse(PromotionId),
            ComboPromotionId = null
        };

        await _saleOrderDetailsRepository.CreateSaleOrderDetail(saleOrderDetail);

        var productQuantity = await _productQuantitiesRepository.GetProductQuantitiesByProductId(Guid.Parse(ProductId), SelectedSize);
        if (productQuantity == null)
        {
            return Result.Failure(new Error("ProductQuantityNotFound", "Product quantity not found"));
        }

        productQuantity.Quantities -= Quantity;

        await _productQuantitiesRepository.UpdateProductQuantity(productQuantity);

        return Result.Success();
    }

    public async Task<Result> CreateSaleOrderDetailForComboPromotion(
        string SaleOrderId,
        string ProductId,
        string SelectedSize,
        int TotalProductQuantity,
        int ComboQuantity,
        string PromotionId
    )
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(ProductId));
        if (product == null)
            return Result.Failure(new Error("ProductNotFound", "Product not found"));

        var comboPromotion = await _comboPromotionsRepository.GetComboPromotionById(Guid.Parse(PromotionId));
        if (comboPromotion == null)
            return Result.Failure(new Error("ComboPromotionNotFound", "Combo promotion not found"));

        var grossComboValue = comboPromotion.ComboPromotionDetails.Sum(d => d.Product.SalePrice * d.Quantity);

        var thisItemOriginalValue = product.SalePrice * (TotalProductQuantity / ComboQuantity);
        var proportion = grossComboValue > 0 ? thisItemOriginalValue / grossComboValue : 0;

        var allocatedPricePerCombo = Math.Round(comboPromotion.ComboPrice * proportion, 2);
        var unitPrice = Math.Round(allocatedPricePerCombo / (TotalProductQuantity / ComboQuantity), 2);
        var subTotal = Math.Round(unitPrice * TotalProductQuantity, 2);
        var profit = Math.Round(subTotal - (product.ImportPrice * TotalProductQuantity), 2);

        var saleOrderDetail = new SaleOrderDetail
        {
            Id = Guid.NewGuid(),
            SaleOrderId = Guid.Parse(SaleOrderId),
            ProductId = Guid.Parse(ProductId),
            SelectedSize = SelectedSize,
            Quantity = TotalProductQuantity,
            Discount = 0,
            UnitPrice = unitPrice,
            SubTotal = subTotal,
            Profit = profit,
            ProductPromotionId = null,
            ComboPromotionId = Guid.Parse(PromotionId)
        };

        await _saleOrderDetailsRepository.CreateSaleOrderDetail(saleOrderDetail);

        var productQuantity = await _productQuantitiesRepository.GetProductQuantitiesByProductId(Guid.Parse(ProductId), SelectedSize);

        if (productQuantity == null)
        {
            return Result.Failure(new Error("ProductQuantityNotFound", "Product quantity not found"));
        }

        productQuantity.Quantities -= TotalProductQuantity;

        await _productQuantitiesRepository.UpdateProductQuantity(productQuantity);

        return Result.Success();
    }
}