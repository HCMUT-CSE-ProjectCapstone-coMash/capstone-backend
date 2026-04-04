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
}