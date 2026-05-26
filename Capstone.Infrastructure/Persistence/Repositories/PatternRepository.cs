namespace Capstone.Infrastructure.Persistence.Repositories;

using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class PatternRepository : IPatternRepository
{
    private readonly AppDbContext _context;

    public PatternRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pattern>> FetchPatterns()
    {
        return await _context.Patterns.ToListAsync();
    }
}