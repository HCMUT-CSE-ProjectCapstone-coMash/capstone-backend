using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ISaleOrderDetailsRepository
{
    Task CreateSaleOrderDetail(SaleOrderDetail saleOrderDetail);
    Task<bool> ExistsByProductId(Guid productId);
    
}