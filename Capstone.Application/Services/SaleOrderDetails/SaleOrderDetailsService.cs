using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.SaleOrderDetails;

public class SaleOrderDetailsService : ISaleOrderDetailsService
{
    private readonly ISaleOrderDetailsRepository _saleOrderDetailsRepository;
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;
    private readonly IProductsRepository _productsRepository;

    public SaleOrderDetailsService(
        ISaleOrderDetailsRepository saleOrderDetailsRepository,
        IProductQuantitiesRepository productQuantitiesRepository,
        IProductsRepository productsRepository
    )
    {
        _saleOrderDetailsRepository = saleOrderDetailsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
        _productsRepository = productsRepository;
    }

    public async Task<Result> CreateSaleOrderDetail(
        string SaleOrderId,
        string ProductId,
        string SelectedSize,
        int Quantity,
        double Discount
    )
    {
        var product = await _productsRepository.GetProductById(Guid.Parse(ProductId));
        if (product == null)
        {
            return Result.Failure(new Error("ProductNotFound", "Product not found"));
        }

        var UnitPrice = product.SalePrice;
        var SubTotal = Quantity * UnitPrice * (1 - Discount / 100);

        var saleOrderDetail = new SaleOrderDetail
        {
            Id = Guid.NewGuid(),
            SaleOrderId = Guid.Parse(SaleOrderId),
            ProductId = Guid.Parse(ProductId),
            SelectedSize = SelectedSize,
            Quantity = Quantity,
            Discount = Discount,
            UnitPrice = UnitPrice,
            SubTotal = SubTotal
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
}