using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ComboPromotionsRepository : IComboPromotionsRepository
{
    private readonly AppDbContext _context;

    public ComboPromotionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateComboPromotion(ComboPromotion comboPromotion)
    {
        _context.ComboPromotions.Add(comboPromotion);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ComboPromotion>> GetComboPromotionsByPromotionId(Guid promotionId)
    {
        return await _context.ComboPromotions
            .AsNoTracking()
            .Include(co => co.ComboPromotionDetails)
                .ThenInclude(cod => cod.Product)
                    .ThenInclude(p => p.ProductQuantities)
            .Where(co => co.PromotionId == promotionId)
            .ToListAsync();
    }
}