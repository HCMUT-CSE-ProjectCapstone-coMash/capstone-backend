using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

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
}