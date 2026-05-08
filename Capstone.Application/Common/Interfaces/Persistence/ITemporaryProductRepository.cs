using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ITemporaryProductsRepository
{
    Task CreateTemporaryProduct(TemporaryProduct temporaryProduct);

    Task<List<TemporaryProduct>> GetTemporaryProductsByUserId(Guid UserId);

    Task DeleteTemporaryProduct(Guid TemporaryProductId);

    Task<TemporaryProduct?> GetTemporaryProductById(Guid TemporaryProductId);
}