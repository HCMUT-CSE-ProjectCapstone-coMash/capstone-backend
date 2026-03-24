using Capstone.Application.Common.Interfaces.Persistence;
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

    public async Task<ProductsOrder?> GetProductsOrdersByCreatedByAndStatus(Guid createdBy, string status)
    {
        return await _context.ProductsOrders
            .Include(po => po.ProductsOrdersDetails)
            .ThenInclude(detail => detail.Product)
            .ThenInclude(p => p.ProductQuantities)
            .AsSplitQuery()
            .FirstOrDefaultAsync(po => po.CreatedBy == createdBy && po.OrderStatus == status);
    }

    public async Task CreateProductsOrders(ProductsOrder ProductsOrder)
    {
        _context.ProductsOrders.Add(ProductsOrder);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductsOrder?> GetProductsOrdersByOrderId(Guid orderId)
    {
        return await _context.ProductsOrders.FirstOrDefaultAsync(po => po.Id == orderId);
    }

    public async Task PatchProductsOrders(ProductsOrder productsOrder)
    {
        _context.ProductsOrders.Update(productsOrder);
        await _context.SaveChangesAsync();
    }
}