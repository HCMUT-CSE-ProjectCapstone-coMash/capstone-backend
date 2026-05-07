using Capstone.Application.Common;

namespace Capstone.Application.Services.TemporaryProducts;

public interface ITemporaryProductsService
{
    Task<Result> CreateTemporaryProduct(string ImageBase64, string ImageKey);
}