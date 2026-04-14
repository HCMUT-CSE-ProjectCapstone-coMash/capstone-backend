using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.ProductQuantitesService;

public class ProductQuantitiesService : IProductQuantitiesService
{
    private readonly IProductQuantitiesRepository _productQuantitiesRepository;

    public ProductQuantitiesService(IProductQuantitiesRepository productQuantitiesRepository)
    {
        _productQuantitiesRepository = productQuantitiesRepository;
    }

    public async Task<Result> CreateProductQuantities(string productId, string size, int quantity)
    {
        var productQuantity = new ProductQuantity
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.Parse(productId),
            Size = size,
            Quantities = quantity
        };

        await _productQuantitiesRepository.AddProductQuantities(productQuantity);

        return Result.Success();
    }
}