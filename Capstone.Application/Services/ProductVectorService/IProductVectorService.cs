using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductVectorService;

public interface IProductVectorService
{
    Task<Result<string>> InsertImageAsync(string imageUrl, string productId);

    Task<Result> DeleteImageAsync(string vectorId);
}