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

    public async Task<(List<ProductsOrder> Items, int Total)> GetProductsOrdersExcludingPending(int currentPage, int pageSize, string? search = null)
    {
        var query = _context.ProductsOrders
            .Where(po => po.OrderStatus != ProductsOrderStatus.Pending);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";
            query = query.Where(po =>
                EF.Functions.ILike(
                    EF.Functions.Unaccent(po.OrderName),
                    EF.Functions.Unaccent(searchPattern)
                ));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
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

    public async Task DeleteProductsOrder(ProductsOrder productsOrder)
    {
        _context.ProductsOrders.Remove(productsOrder);
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

    public async Task<bool> ExistsByEmployeeId(Guid employeeId)
    {
        return await _context.ProductsOrders
            .AnyAsync(user => user.CreatedBy == employeeId);
    }
}