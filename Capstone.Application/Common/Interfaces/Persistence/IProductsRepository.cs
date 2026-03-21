using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsRepository
{
    Task AddProduct(Product product);
    Task<Product?> GetProductById(Guid productId);
    Task DeleteProductAsync(Guid productId);
}