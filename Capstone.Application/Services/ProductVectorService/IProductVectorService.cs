using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductVectorService;

public interface IProductVectorService
{
    Task<Result<string>> InsertImageAsync(string imageUrl, string productId);
}