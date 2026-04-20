using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IComboPromotionDetailsRepository
{
    Task CreateComboPromotionDetail(ComboPromotionDetail comboPromotionDetail);
}