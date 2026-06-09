using Capstone.Application.Common;
using Capstone.Application.Services.Products;

namespace Capstone.Application.Services.Vectorize;

public interface IVectorizeService
{
    Task<Result<float[]>> VectorizeImageAsync(string imageUrl);
    Task<Result<List<ProductWithOrderStatusDto>>> FetchSimilarProducts(string imageBase64);
}