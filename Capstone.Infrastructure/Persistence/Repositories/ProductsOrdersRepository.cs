using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ProductsOrdersRepository : IProductsOrdersRepository
{
    private readonly AppDbContext _context;

    public ProductsOrdersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductsOrder?> GetProductsOrdersByCreatedBy(Guid createdBy)
    {
        return await _context.ProductsOrders
            .Include(po => po.ProductsOrdersDetails)
                .ThenInclude(detail => detail.Product)
                    .ThenInclude(p => p.ProductQuantities)
            .Include(po => po.ProductsOrdersDetails)
                .ThenInclude(detail => detail.QuantityChanges)
            .AsSplitQuery()
            .FirstOrDefaultAsync(po => po.CreatedBy == createdBy
                && po.OrderStatus == ProductsOrderStatus.Pending);
    }

    public async Task<List<ProductsOrder>> GetProductsOrdersExcludingPending()
    {
        return await _context.ProductsOrders.Where(po => po.OrderStatus != "Pending").ToListAsync();
    }

    public async Task CreateProductsOrders(ProductsOrder ProductsOrder)
    {
        _context.ProductsOrders.Add(ProductsOrder);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductsOrder?> GetProductsOrdersByOrderId(Guid orderId)
    {
        return await _context.ProductsOrders
            .Include(po => po.ProductsOrdersDetails)
                .ThenInclude(detail => detail.Product)
                    .ThenInclude(p => p.ProductQuantities)
            .Include(po => po.ProductsOrdersDetails)
                .ThenInclude(detail => detail.QuantityChanges)
            .AsSplitQuery()
            .FirstOrDefaultAsync(po => po.Id == orderId);
    }

    public async Task PatchProductsOrders(ProductsOrder productsOrder)
    {
        _context.ProductsOrders.Update(productsOrder);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetProductIdsInPendingAndSendingOrders()
    {
        return await _context.ProductsOrders
            .Where(po => po.OrderStatus == ProductsOrderStatus.Pending || po.OrderStatus == ProductsOrderStatus.Sending)
            .SelectMany(po => po.ProductsOrdersDetails.Select(detail => detail.ProductId))
            .Distinct()
            .ToListAsync();
    }
}