using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsOrdersDetailsQuantityChangesRepository
{
    Task AddQuantityChange(ProductsOrdersDetailQuantityChange quantityChange);
    Task DeleteQuantityChangesByProductsOrdersDetailId(Guid productsOrdersDetailId);
}