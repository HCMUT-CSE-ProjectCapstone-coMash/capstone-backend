using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.ComboPromotionsService;

public class ComboPromotionsService : IComboPromotionsService
{
    private readonly IComboPromotionsRepository _comboPromotionsRepository;
    private readonly IComboPromotionDetailsRepository _comboPromotionDetailsRepository;

    public ComboPromotionsService(
        IComboPromotionsRepository comboPromotionsRepository,
        IComboPromotionDetailsRepository comboPromotionDetailsRepository
    )
    {
        _comboPromotionsRepository = comboPromotionsRepository;
        _comboPromotionDetailsRepository = comboPromotionDetailsRepository;
    }

    public async Task<Result<string>> CreateComboPromotion(string promotionId, string comboName, decimal comboPrice)
    {
        var newComboPromotion = new ComboPromotion
        {
            Id = Guid.NewGuid(),
            PromotionId = Guid.Parse(promotionId),
            ComboName = comboName,
            ComboPrice = comboPrice
        };

        await _comboPromotionsRepository.CreateComboPromotion(newComboPromotion);

        return Result<string>.Success(newComboPromotion.Id.ToString());
    }

    public async Task<Result> CreateComboPromotionDetail(string comboPromotionId, string productId, int quantity)
    {
        var newComboPromotionDetail = new ComboPromotionDetail
        {
            Id = Guid.NewGuid(),
            ComboPromotionId = Guid.Parse(comboPromotionId),
            ProductId = Guid.Parse(productId),
            Quantity = quantity
        };

        await _comboPromotionDetailsRepository.CreateComboPromotionDetail(newComboPromotionDetail);

        return Result.Success();
    }
}