using Capstone.Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class PromotionsRepository : IPromotionsRepository
{
    private readonly AppDbContext _context;

    public PromotionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetMaxPromotionId(string prefix)
    {
        var numberStrings = await _context.Promotions
            .Where(p => p.PromotionId != null && p.PromotionId.StartsWith(prefix + "-"))
            .Select(p => p.PromotionId!.Substring(prefix.Length + 1))
            .ToListAsync();

        if (!numberStrings.Any()) return 0;

        return numberStrings.Max(n => int.TryParse(n, out var num) ? num : 0);
    }
}