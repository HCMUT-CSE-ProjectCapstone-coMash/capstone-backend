using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class productsOrdersDetailsQuantityChangesRepository : IProductsOrdersDetailsQuantityChangesRepository
{
    private readonly AppDbContext _context;

    public productsOrdersDetailsQuantityChangesRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddQuantityChange(ProductsOrdersDetailQuantityChange quantityChange)
    {
        _context.ProductsOrdersDetailQuantityChanges.Add(quantityChange);
        return _context.SaveChangesAsync();
    }

    public Task DeleteQuantityChangesByProductsOrdersDetailId(Guid productsOrdersDetailId)
    {
        var quantityChanges = _context.ProductsOrdersDetailQuantityChanges.Where(qc => qc.ProductsOrdersDetailId == productsOrdersDetailId);
        _context.ProductsOrdersDetailQuantityChanges.RemoveRange(quantityChanges);
        return _context.SaveChangesAsync();
    }
}