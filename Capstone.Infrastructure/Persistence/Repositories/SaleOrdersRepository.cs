using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class SaleOrdersRepository : ISaleOrdersRepository
{
    private readonly AppDbContext _context;

    public SaleOrdersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateSaleOrder(SaleOrder saleOrder)
    {
        _context.SaleOrders.Add(saleOrder);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSaleOrder(SaleOrder saleOrder)
    {
        _context.SaleOrders.Update(saleOrder);
        await _context.SaveChangesAsync();
    }

    public async Task<SaleOrder?> GetSaleOrderWithDetails(Guid saleOrderId)
    {
        return await _context.SaleOrders
            .Include(so => so.SaleOrderDetails)
                .ThenInclude(d => d.Product)
            .Include(so => so.SaleOrderDetails)
                .ThenInclude(d => d.ProductPromotion)
                    .ThenInclude(pp => pp!.Promotion)
            .Include(so => so.SaleOrderDetails)
                .ThenInclude(d => d.ComboPromotion)
                    .ThenInclude(cp => cp!.Promotion)
            .Include(so => so.Customer)
            .Include(so => so.User)
            .FirstOrDefaultAsync(so => so.Id == saleOrderId);
    }

    public async Task<int> GetMaxIdNumber()
    {
        var lastId = await _context.SaleOrders
            .OrderByDescending(so => so.CreatedAt)
            .Select(so => so.SaleOrderId)
            .FirstOrDefaultAsync();

        if (lastId is null || !int.TryParse(lastId.Substring(3), out var number))
            return 0;

        return number;
    }

    public async Task<bool> ExistsByEmployeeId(Guid employeeId)
    {
        return await _context.SaleOrders.AnyAsync(saleOrder => saleOrder.CreatedBy == employeeId);
    }

    public async Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrders(int page, int pageSize, string? search = null)
    {
        var query = _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(
                so => so.SaleOrderId.Contains(search) 
                || EF.Functions.Unaccent(so.Customer!.CustomerName).Contains(EF.Functions.Unaccent(search)));
        }

        var total = await query.CountAsync();
        var orders = await query
            .OrderByDescending(so => so.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, total);
    }
}