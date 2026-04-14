using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> ExistsByProductId(Guid productId)
    {
        return await _context.SaleOrderDetails.AnyAsync(sod => sod.ProductId == productId);
    }

    public async Task<bool> ExistsByEmployeeId(Guid employeeId)
    {
        return await _context.SaleOrderDetails
            .Join(_context.SaleOrders,
                detail => detail.SaleOrderId,
                order => order.Id,
                (detail, order) => order.CreatedBy)
            .Join(_context.Users,
                createdBy => createdBy,
                user => user.Id,
                (createdBy, user) => user)
            .AnyAsync(user => user.Id == employeeId && user.Role == Roles.Employee);
    }
}