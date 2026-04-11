using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.SaleOrderDetails;

public class SaleOrderDetailsService : ISaleOrderDetailsService
{
    private readonly ISaleOrderDetailsRepository _saleOrderDetailsRepository;
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;

    public SaleOrderDetailsService(
        ISaleOrderDetailsRepository saleOrderDetailsRepository,
        IProductQuantitiesRepository productQuantitiesRepository
    )
    {
        _saleOrderDetailsRepository = saleOrderDetailsRepository;
        _productQuantitiesRepository = productQuantitiesRepository;
    }

    public async Task<Result> CreateSaleOrderDetail(
        string SaleOrderId,
        string ProductId,
        string SelectedSize,
        int Quantity,
        double Discount
    )
    {
        var saleOrderDetail = new SaleOrderDetail
        {
            Id = Guid.NewGuid(),
            SaleOrderId = Guid.Parse(SaleOrderId),
            ProductId = Guid.Parse(ProductId),
            SelectedSize = SelectedSize,
            Quantity = Quantity,
            Discount = Discount
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