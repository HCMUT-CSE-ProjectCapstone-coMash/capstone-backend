using Capstone.Application.Common;

namespace Capstone.Application.Services.Promotions;

public interface IPromotionsService
{
    Task<Result<string>> CreatePromotionId();
}