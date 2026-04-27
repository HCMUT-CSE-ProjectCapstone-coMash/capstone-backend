using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
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

    public async Task CreatePromotion(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Promotion> Items, int Total)> FetchPromotions(int page, int pageSize, string? category = null, string? search = null)
    {
        var query = _context.Promotions.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.PromotionType == category);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchPattern = $"%{search}%";
            query = query.Where(p =>
                // match against promotion name (accent-insensitive)
                EF.Functions.ILike(
                    EF.Functions.Unaccent(p.PromotionName),
                    EF.Functions.Unaccent(searchPattern))
                ||
                // match "KM-6" style against promotionId
                (p.PromotionId != null && EF.Functions.ILike(p.PromotionId, searchPattern))
            );
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Promotion?> GetPromotionById(Guid promotionId)
    {
        return await _context.Promotions.FirstOrDefaultAsync(p => p.Id == promotionId);
    }

    public async Task UpdatePromotion(Promotion promotion)
    {
        _context.Promotions.Update(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Promotion>> GetProductPromotionsByProductId(Guid productId)
    {
        return await _context.Promotions
            .Where(p => p.ProductPromotions.Any(pp => pp.ProductId == productId))
            .Include(p => p.ProductPromotions)
                .ThenInclude(pp => pp.Product)
                    .ThenInclude(p => p.ProductQuantities)
            .ToListAsync();
    }

    public async Task<List<Promotion>> GetComboPromotionsByProductId(Guid productId)
    {
        return await _context.Promotions
            .Where(p => p.ComboPromotions.Any(cp => cp.ComboPromotionDetails.Any(cpd => cpd.ProductId == productId)))
            .Include(p => p.ComboPromotions)
                .ThenInclude(cp => cp.ComboPromotionDetails)
                    .ThenInclude(cpd => cpd.Product)
                        .ThenInclude(p => p.ProductQuantities)
            .ToListAsync();
    }

    public async Task<List<Promotion>> GetOrderPromotions()
    {
        return await _context.Promotions.Where(p => p.PromotionType == PromotionType.OrderPromotion).Include(p => p.OrderPromotions).ToListAsync();
    }
}