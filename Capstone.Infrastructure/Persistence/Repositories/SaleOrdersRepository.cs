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
            .Include(so => so.SaleOrderDetails)
                .ThenInclude(d => d.ComboPromotion)
                    .ThenInclude(cp => cp!.ComboPromotionDetails)
                        .ThenInclude(cpd => cpd.Product)
            .Include(so => so.AppliedOrderPromotion)
                .ThenInclude(op => op!.Promotion)
            .Include(so => so.Customer)
            .Include(so => so.User)
            .FirstOrDefaultAsync(so => so.Id == saleOrderId);
    }

    private static (DateTime? Start, DateTime? End) ResolvePeriodRange(string? period)
    {
        if (string.IsNullOrWhiteSpace(period))
            return (null, null);

        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        var today = DateOnly.FromDateTime(nowVN);

        static DateTime UtcFromVn(DateOnly date, TimeZoneInfo timeZone)
        {
            var localMidnight = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(localMidnight, timeZone);
        }

        var normalized = period.Trim().ToLowerInvariant();
        DateTime? start = null;
        DateTime? end = null;

        if (normalized == "today")
        {
            start = UtcFromVn(today, vnTimeZone);
            end = UtcFromVn(today.AddDays(1), vnTimeZone);
        }
        else if (normalized == "yesterday")
        {
            start = UtcFromVn(today.AddDays(-1), vnTimeZone);
            end = UtcFromVn(today, vnTimeZone);
        }
        else if (normalized == "this_week")
        {
            var startOfWeek = today.AddDays(-((int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1));
            start = UtcFromVn(startOfWeek, vnTimeZone);
            end = UtcFromVn(startOfWeek.AddDays(7), vnTimeZone);
        }
        else if (normalized == "this_month")
        {
            var startOfMonth = new DateOnly(today.Year, today.Month, 1);
            start = UtcFromVn(startOfMonth, vnTimeZone);
            end = UtcFromVn(startOfMonth.AddMonths(1), vnTimeZone);
        }

        return (start, end);
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

    public async Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrders(int page, int pageSize, string? timeRange = null, string? search = null)
    {
        var query = _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var searchPattern = $"%{search}%";

            query = query.Where(
                so => EF.Functions.ILike(so.SaleOrderId, searchPattern)
                || EF.Functions.ILike(
                    EF.Functions.Unaccent(so.Customer!.CustomerName),
                    EF.Functions.Unaccent(searchPattern)));
        }

        var (start, end) = ResolvePeriodRange(timeRange);
        if (start.HasValue && end.HasValue)
        {
            query = query.Where(so => so.CreatedAt >= start.Value && so.CreatedAt < end.Value);
        }
        
        var total = await query.CountAsync();
        var orders = await query
            .OrderByDescending(so => so.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, total);
    }

    public async Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrdersByEmployeeId(Guid employeeId, int page, int pageSize, string? search = null)
    {
        var query = _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.User)
            .Where(so => so.CreatedBy == employeeId);

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

    public async Task<(List<SaleOrder> Items, int Total)> FetchAllSaleOrdersByCustomerId(Guid customerId, int page, int pageSize, string? search = null)
    {
        var query = _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.User)
            .Where(so => so.CustomerId == customerId);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(
                so => so.SaleOrderId.Contains(search)
                || EF.Functions.Unaccent(so.User.FullName).Contains(EF.Functions.Unaccent(search)));
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