using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductsOrdersDetailsRepository
{
    Task CreateProductsOrdersDetails(ProductsOrdersDetail productsOrdersDetail);
    Task<ProductsOrdersDetail?> GetProductsOrdersDetailsByOrderIdAndProductId(Guid orderId, Guid productId);
    Task DeleteProductsOrdersDetails(Guid productsOrdersDetailId);
    Task<bool> ExistsByProductId(Guid productId);
    Task<bool> ExistsByEmployeeId(Guid employeeId);
}