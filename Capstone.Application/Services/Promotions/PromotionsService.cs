using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;

namespace Capstone.Application.Services.Promotions;

public class PromotionsService : IPromotionsService
{
    private readonly IPromotionsRepository _promotionsRepository;

    public PromotionsService(IPromotionsRepository promotionsRepository)
    {
        _promotionsRepository = promotionsRepository;
    }

    public async Task<Result<string>> CreatePromotionId()
    {
        var prefix = "KM";
        
        var maxNumber = await _promotionsRepository.GetMaxPromotionId(prefix);
        var newId = $"{prefix}-{maxNumber + 1:D5}";

        return Result<string>.Success(newId);
    }
}