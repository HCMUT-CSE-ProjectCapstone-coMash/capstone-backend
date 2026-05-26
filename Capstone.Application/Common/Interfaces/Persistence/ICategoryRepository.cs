using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ICategoryRepository
{
    Task<List<Category>> FetchCategories();
    Task<Category?> GetCategoryById(Guid Id);
}