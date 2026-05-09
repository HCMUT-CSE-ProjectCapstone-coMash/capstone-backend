using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class SaleOrdersRepository : ISaleOrdersRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SaleOrdersRepository(AppDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
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

        var now = _dateTimeProvider.UtcNow;

        query = timeRange switch
        {
            "today" => query.Where(so =>
                so.CreatedAt.Date == now.Date),

            "yesterday" => query.Where(so =>
                so.CreatedAt.Date == now.Date.AddDays(-1)),

            "this_week" => query.Where(so =>
                so.CreatedAt >= now.Date.AddDays(-(int)now.DayOfWeek)
                && so.CreatedAt < now.Date.AddDays(7 - (int)now.DayOfWeek)),

            "this_month" => query.Where(so =>
                so.CreatedAt.Year == now.Year
                && so.CreatedAt.Month == now.Month),

            _ => query
        };

        if (!string.IsNullOrEmpty(search))
        {
            var searchPattern = $"%{search}%";

            query = query.Where(
                so => EF.Functions.ILike(so.SaleOrderId, searchPattern)
                || EF.Functions.ILike(
                    EF.Functions.Unaccent(so.Customer!.CustomerName),
                    EF.Functions.Unaccent(searchPattern)));
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

    public async Task<List<SaleOrder>> GetAllSaleOrdersWithDebt(Guid customerId)
    {
        return await _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.User)
            .Where(so => so.DebitMoney > 0 && so.CustomerId == customerId)
            .OrderByDescending(so => so.CreatedAt)
            .ToListAsync();
    }

    public async Task<IncomeStatsDto> GetIncomeStats(string period)
    {
        var now = _dateTimeProvider.UtcNow.AddHours(7);

        var jsDay = (int)now.DayOfWeek;  
        var daysToMonday = jsDay == 0 ? -6 : 1 - jsDay;
        var weekStart = now.Date.AddDays(daysToMonday);      
        var weekEnd   = weekStart.AddDays(7);                    

        var weekStartUtc = weekStart.AddHours(-7);
        var weekEndUtc   = weekEnd.AddHours(-7);

        var query = _context.SaleOrders.AsQueryable();

        query = period switch
        {
            "day"     => query.Where(so => so.CreatedAt >= weekStartUtc && so.CreatedAt < weekEndUtc),
            "week"    => query.Where(so => so.CreatedAt.Month == now.Month && so.CreatedAt.Year == now.Year),
            "month"   => query.Where(so => so.CreatedAt.Year == now.Year),
            "quarter" => query.Where(so => so.CreatedAt.Year == now.Year),
            _         => query.Where(so => so.CreatedAt >= weekStartUtc && so.CreatedAt < weekEndUtc),
        };

        var orders = await query
            .Where(so => so.TotalPrice > 0)
            .Select(so => new { so.CreatedAt, so.TotalPrice, so.TotalProfit })
            .ToListAsync();

        var vietnamOrders = orders
            .Select(o => new { CreatedAt = o.CreatedAt.AddHours(7), o.TotalPrice, o.TotalProfit})
            .ToList();

        var groups = period switch
        {
            "day" => vietnamOrders
                .GroupBy(so => so.CreatedAt.DayOfWeek)
                .Select(g => new IncomeGroupDto
                {
                    Key = g.Key == DayOfWeek.Sunday ? "7" : ((int)g.Key).ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                    Profit = g.Sum(o => o.TotalProfit),
                })
                .ToList(),

            "week" => vietnamOrders
                .GroupBy(so => GetWeekOfMonth(so.CreatedAt))
                .Select(g => new IncomeGroupDto
                {
                    Key = g.Key.ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                    Profit = g.Sum(o => o.TotalProfit),
                })
                .ToList(),

            "month" => vietnamOrders
                .GroupBy(so => so.CreatedAt.Month)
                .Select(g => new IncomeGroupDto
                {
                    Key = g.Key.ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                    Profit = g.Sum(o => o.TotalProfit),
                })
                .ToList(),

            "quarter" => vietnamOrders
                .GroupBy(so => (so.CreatedAt.Month - 1) / 3 + 1)
                .Select(g => new IncomeGroupDto
                {
                    Key = g.Key.ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                    Profit = g.Sum(o => o.TotalProfit),
                })
                .ToList(),

            _ => []
        };

        return new IncomeStatsDto
        {
            Period = period,
            Total = vietnamOrders.Sum(o => o.TotalPrice),
            TotalProfit = vietnamOrders.Sum(o => o.TotalProfit),
            Groups = groups
        };
    }

    public async Task<PersonalIncomeStatsDto> GetPersonalIncomeStats(Guid employeeId, string period)
    {
        var now = _dateTimeProvider.UtcNow.AddHours(7);

        var jsDay = (int)now.DayOfWeek;  
        var daysToMonday = jsDay == 0 ? -6 : 1 - jsDay;
        var weekStart = now.Date.AddDays(daysToMonday);      
        var weekEnd   = weekStart.AddDays(7);                    

        var weekStartUtc = weekStart.AddHours(-7);
        var weekEndUtc   = weekEnd.AddHours(-7);

        var query = _context.SaleOrders
            .Where(so => so.CreatedBy == employeeId);

        query = period switch
        {
            "day"     => query.Where(so => so.CreatedAt >= weekStartUtc && so.CreatedAt < weekEndUtc),
            "week"    => query.Where(so => so.CreatedAt.Month == now.Month && so.CreatedAt.Year == now.Year),
            "month"   => query.Where(so => so.CreatedAt.Year == now.Year),
            "quarter" => query.Where(so => so.CreatedAt.Year == now.Year),
            _         => query.Where(so => so.CreatedAt >= weekStartUtc && so.CreatedAt < weekEndUtc),
        };

        var orders = await query
            .Where(so => so.TotalPrice > 0)
            .Select(so => new { so.CreatedAt, so.TotalPrice })
            .ToListAsync();

        var vietnamOrders = orders
            .Select(o => new { CreatedAt = o.CreatedAt.AddHours(7), o.TotalPrice})
            .ToList();

        var groups = period switch
        {
            "day" => vietnamOrders
                .GroupBy(so => so.CreatedAt.DayOfWeek)
                .Select(g => new PersonalIncomeGroupDto
                {
                    Key = g.Key == DayOfWeek.Sunday ? "7" : ((int)g.Key).ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                })
                .ToList(),

            "week" => vietnamOrders
                .GroupBy(so => GetWeekOfMonth(so.CreatedAt))
                .Select(g => new PersonalIncomeGroupDto
                {
                    Key = g.Key.ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                })
                .ToList(),

            "month" => vietnamOrders
                .GroupBy(so => so.CreatedAt.Month)
                .Select(g => new PersonalIncomeGroupDto
                {
                    Key = g.Key.ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                })
                .ToList(),

            "quarter" => vietnamOrders
                .GroupBy(so => (so.CreatedAt.Month - 1) / 3 + 1)
                .Select(g => new PersonalIncomeGroupDto
                {
                    Key = g.Key.ToString(),
                    Total = g.Sum(o => o.TotalPrice),
                })
                .ToList(),

            _ => []
        }; 
        
        return new PersonalIncomeStatsDto
        {
            Period = period,
            Total = vietnamOrders.Sum(o => o.TotalPrice),
            Groups = groups,
        };
    }

    public async Task<TopCustomerStatsDto> GetTopCustomersSpendingStats(int limit)
    {
        var grandTotal = await _context.SaleOrders.Where(so => so.TotalPrice > 0).SumAsync(so => so.TotalPrice);

        var topCustomers = await _context.SaleOrders
            .Where(so => so.TotalPrice > 0 && so.CustomerId != null)
            .GroupBy(so => new { so.CustomerId, so.Customer!.CustomerName })
            .Select(g => new TopCustomerDto
            {
                CustomerId = g.Key.CustomerId!.Value,
                Name = g.Key.CustomerName,
                Total = g.Sum(o => o.TotalPrice),
            })
            .OrderByDescending(x => x.Total)
            .Take(limit)
            .ToListAsync();

        var walkInTotal = await _context.SaleOrders
            .Where(so => so.TotalPrice > 0 && so.CustomerId == null)
            .SumAsync(so => so.TotalPrice);

        return new TopCustomerStatsDto
        {
            Customers = topCustomers,
            WalkInTotal = walkInTotal,
            GrandTotal = grandTotal,
        };
    }

    public async Task<DashboardStatsDto> GetDashboardStats()
    {
        var now = _dateTimeProvider.UtcNow.AddHours(7);

        var todayStart = now.Date.AddHours(-7);
        var todayEnd = todayStart.AddDays(1);
        var yesterdayStart = todayStart.AddDays(-1);
        var yesterdayEnd = todayStart;

        var todayOrders = await _context.SaleOrders
            .Where(so => so.CreatedAt >= todayStart && so.CreatedAt < todayEnd && so.TotalPrice > 0)
            .Select(so => new { so.TotalPrice, so.TotalProfit })
            .ToListAsync();

        var yesterdayOrders = await _context.SaleOrders
            .Where(so => so.CreatedAt >= yesterdayStart && so.CreatedAt < yesterdayEnd && so.TotalPrice > 0)
            .Select(so => new { so.TotalPrice, so.TotalProfit })
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalSaleToday = todayOrders.Sum(o => o.TotalPrice),
            ProfitToday = todayOrders.Sum(o => o.TotalProfit),
            TotalOrderToday = todayOrders.Count,
            TotalSaleYesterday = yesterdayOrders.Sum(o => o.TotalPrice),
            ProfitYesterday = yesterdayOrders.Sum(o => o.TotalProfit),
            TotalOrderYesterday = yesterdayOrders.Count,
        };
    }

    public async Task<List<SaleOrder>> FetchRecentCreatedByEmployee(Guid employeeId)
    {
        return await _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.User)
            .Where(so => so.CreatedBy == employeeId)
            .OrderByDescending(so => so.CreatedAt)
            .Take(5)
            .ToListAsync();
    }

    private static int GetWeekOfMonth(DateTime date)
    {
        var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

        var firstDayOffset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

        return (date.Day + firstDayOffset - 1) / 7 + 1;
    }
}