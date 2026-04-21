using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IComboPromotionsRepository
{
    Task CreateComboPromotion(ComboPromotion comboPromotion);

    Task<List<ComboPromotion>> GetComboPromotionsByPromotionId(Guid promotionId);
}