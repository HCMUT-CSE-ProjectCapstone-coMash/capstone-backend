using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductQuantitiesRepository
{
    Task AddProductQuantities(ProductQuantities productQuantities);
    Task DeleteByProductId(Guid productId);
}