using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IPromotionsRepository
{
    Task<int> GetMaxPromotionId(string prefix);

    Task CreatePromotion(Promotion promotion);

    Task<(List<Promotion> Items, int Total)> FetchPromotions(int page, int pageSize, string? category = null, string? search = null);

    Task<Promotion?> GetPromotionById(Guid promotionId);

    Task UpdatePromotion(Promotion promotion);

    Task<List<Promotion>> GetProductPromotionsByProductId(Guid productId);
    
    Task<List<Promotion>> GetComboPromotionsByProductId(Guid productId);
}