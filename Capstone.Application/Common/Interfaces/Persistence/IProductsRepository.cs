using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsRepository
{
    Task AddProduct(Product product);
    Task<Product?> GetProductById(Guid productId);
    Task DeleteProductAsync(Guid productId);
    Task UpdateProduct(Product product);
    Task<int> GetMaxIdNumberByCategoryAsync(string prefix);
    Task<List<Product>> FetchApprovedProductByName(string productName);
    Task<(List<Product> Items, int Total)> FetchAllProducts(int page, int pageSize, string? category = null, string? search = null);
}