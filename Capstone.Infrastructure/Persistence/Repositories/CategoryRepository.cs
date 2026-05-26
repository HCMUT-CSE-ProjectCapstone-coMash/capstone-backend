using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> FetchCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetCategoryById(Guid Id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == Id);
    }
}