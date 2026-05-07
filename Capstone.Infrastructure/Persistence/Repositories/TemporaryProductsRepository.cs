using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class TemporaryProductsRepository : ITemporaryProductsRepository
{
    private readonly AppDbContext _context;

    public TemporaryProductsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateTemporaryProduct(TemporaryProduct temporaryProduct)
    {
        _context.TemporaryProducts.Add(temporaryProduct);
        await _context.SaveChangesAsync();
    }
}