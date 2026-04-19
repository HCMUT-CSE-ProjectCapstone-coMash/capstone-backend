using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IProductPromotionsRepository
{
    Task CreateProductPromotion(ProductPromotion productPromotion);
}