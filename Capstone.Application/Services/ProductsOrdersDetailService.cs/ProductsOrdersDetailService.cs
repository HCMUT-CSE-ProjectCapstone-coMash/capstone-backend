using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;

namespace Capstone.Application.Services.ProductsOrdersDetailService;

public class ProductsOrdersDetailService : IProductsOrdersDetailService
{
    private readonly IProductsOrdersDetailsRepository _productsOrdersDetailRepository;

    public ProductsOrdersDetailService(IProductsOrdersDetailsRepository productsOrdersDetailRepository)
    {
        _productsOrdersDetailRepository = productsOrdersDetailRepository;
    }

    public async Task<Result> CreateProductsOrdersDetail(string orderId, string productId)
    {
        var productsOrdersDetail = new Domain.Entities.ProductsOrdersDetail
        {
            Id = Guid.NewGuid(),
            ProductsOrderId = Guid.Parse(orderId),
            ProductId = Guid.Parse(productId)
        };

        await _productsOrdersDetailRepository.CreateProductsOrdersDetails(productsOrdersDetail);

        return Result.Success();
    }
}