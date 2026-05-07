using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ITemporaryProductsRepository
{
    Task CreateTemporaryProduct(TemporaryProduct temporaryProduct);
}