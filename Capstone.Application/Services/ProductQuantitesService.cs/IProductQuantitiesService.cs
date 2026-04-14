using Capstone.Application.Common;

namespace Capstone.Application.Services.ProductQuantitesService;

public interface IProductQuantitiesService
{
    Task<Result> CreateProductQuantities(string productId, string size, int quantity);
}