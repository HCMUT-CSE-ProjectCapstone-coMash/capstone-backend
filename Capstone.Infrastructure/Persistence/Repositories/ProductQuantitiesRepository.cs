using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ProductQuantitiesRepository : IProductQuantitiesRepository
{
    private readonly AppDbContext _context;

    public ProductQuantitiesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddProductQuantities(ProductQuantities productQuantities)
    {
        _context.ProductQuantities.Add(productQuantities);
        await _context.SaveChangesAsync();
    }
}