using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IPromotionsRepository
{
    Task<int> GetMaxPromotionId(string prefix);
    Task CreatePromotion(Promotion promotion);
}