using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ISaleOrdersRepository
{
    Task CreateSaleOrder(SaleOrder saleOrder);
    Task UpdateSaleOrder(SaleOrder saleOrder);
    Task<SaleOrder?> GetSaleOrderWithDetails(Guid saleOrderId);
}