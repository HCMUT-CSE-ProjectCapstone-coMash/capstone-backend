using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsRepository
{
    Task AddProduct(Product product);
    Task<Product?> GetProductById(Guid id);
    Task UpdateProduct(Product product);
    Task<List<Product>> GetPendingProductsByUserId(Guid userId);
}