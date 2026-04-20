using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ComboPromotionDetailsRepository : IComboPromotionDetailsRepository
{
    private readonly AppDbContext _context;

    public ComboPromotionDetailsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateComboPromotionDetail(ComboPromotionDetail comboPromotionDetail)
    {
        _context.ComboPromotionDetails.Add(comboPromotionDetail);
        await _context.SaveChangesAsync();
    }
}