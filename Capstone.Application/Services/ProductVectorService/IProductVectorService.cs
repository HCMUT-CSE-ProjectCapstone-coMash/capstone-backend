using Capstone.Application.Common;
using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.ProductVectorService;

public interface IProductVectorService
{
    Task<Result<string>> InsertImageAsync(string imageUrl, string productId);

    Task<Result> DeleteImageAsync(string vectorId);

    Task<Result<List<ProductWithOrderStatusDto>>> FetchSimilarProducts(string imageBase64);
}