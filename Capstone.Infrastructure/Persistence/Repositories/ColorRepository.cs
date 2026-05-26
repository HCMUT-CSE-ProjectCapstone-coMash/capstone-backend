using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ColorRepository : IColorRepository
{
    private readonly AppDbContext _context;

    public ColorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Color>> FetchColors()
    {
        return await _context.Colors.ToListAsync();
    }
}