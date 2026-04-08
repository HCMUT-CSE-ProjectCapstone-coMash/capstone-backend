using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductQuantitiesRepository
{
    Task AddProductQuantities(ProductQuantity productQuantities);
    Task UpdateProductQuantity(ProductQuantity productQuantity);
    Task DeleteProductQuantitiesByProductId(Guid productId);
}