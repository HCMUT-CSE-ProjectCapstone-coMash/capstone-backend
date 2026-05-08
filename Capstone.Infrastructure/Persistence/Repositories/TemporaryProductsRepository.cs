using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<TemporaryProduct>> GetTemporaryProductsByUserId(Guid UserId)
    {
        return await _context.TemporaryProducts.Where(tp => tp.CreatedBy == UserId).ToListAsync();
    }

    public async Task DeleteTemporaryProduct(Guid TemporaryProductId)
    {
        var temporaryProduct = await _context.TemporaryProducts.FindAsync(TemporaryProductId);
        if (temporaryProduct != null)
        {
            _context.TemporaryProducts.Remove(temporaryProduct);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<TemporaryProduct?> GetTemporaryProductById(Guid TemporaryProductId)
    {
        return await _context.TemporaryProducts.FindAsync(TemporaryProductId);
    }
}