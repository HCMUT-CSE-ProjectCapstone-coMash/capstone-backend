using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ProductPromotionsRepository : IProductPromotionsRepository
{
    private readonly AppDbContext _context;

    public ProductPromotionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateProductPromotion(ProductPromotion productPromotion)
    {
        _context.ProductPromotions.Add(productPromotion);
        await _context.SaveChangesAsync();
    }
}