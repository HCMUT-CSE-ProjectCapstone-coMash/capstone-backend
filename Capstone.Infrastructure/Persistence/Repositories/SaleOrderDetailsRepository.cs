using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class SaleOrderDetailsRepository : ISaleOrderDetailsRepository
{
    private readonly AppDbContext _context;

    public SaleOrderDetailsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateSaleOrderDetail(SaleOrderDetail saleOrderDetail)
    {
        _context.SaleOrderDetails.Add(saleOrderDetail);
        await _context.SaveChangesAsync();
    }
}