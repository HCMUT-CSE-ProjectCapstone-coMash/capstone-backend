using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class CustomersRepository : ICustomersRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CustomersRepository(AppDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task CreateCustomer(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<Customer?> GetCustomerByPhone(string customerPhone)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerPhoneNumber == customerPhone);
    }

    public async Task<List<Customer>> FetchCustomerByName(string customerName)
    {
        var namePattern = $"%{customerName}%";
        return await _context.Customers
            .Include(c => c.SaleOrders)
            .Where(c => EF.Functions.ILike(
                EF.Functions.Unaccent(c.CustomerName),
                EF.Functions.Unaccent(namePattern)))
            .Take(8)
            .ToListAsync();
    }

    public async Task<List<Customer>> FetchCustomerByPhone(string customerPhone)
    {
        var phonePattern = $"%{customerPhone}%";
        return await _context.Customers
            .Include(c => c.SaleOrders)
            .Where(c => EF.Functions.ILike(c.CustomerPhoneNumber, phonePattern))
            .Take(8)
            .ToListAsync();
    }

    public async Task<(List<Customer> Items, int Total)> FetchCustomers(int page, int pageSize, string? search = null, bool onlyDebt = false)
    {   
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";
            query = query.Where(c => EF.Functions.ILike(EF.Functions.Unaccent(c.CustomerName), EF.Functions.Unaccent(searchPattern))
                || EF.Functions.ILike(c.CustomerPhoneNumber, searchPattern));
        }

        if (onlyDebt)
        {
            query = query.Where(c =>
                c.SaleOrders.Any(so => so.DebitMoney > 0));
        }

        var total = await query.CountAsync();

        var items = await query
            .Include(c => c.SaleOrders)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Customer?> GetCustomerById(Guid customerId)
    {
        return await _context.Customers
            .Include(c => c.SaleOrders)
            .FirstOrDefaultAsync(c => c.Id == customerId);
    }

    public async Task<List<Customer>> FetchTop5DebtCustomers()
    {
        return await _context.Customers
            .Include(c => c.SaleOrders)
            .Where(c => c.SaleOrders.Any(so => so.PaymentMethod == PaymentMethodStatus.Debit && so.DebitMoney > 0))
            .OrderByDescending(c => c.SaleOrders.Where(so => so.PaymentMethod == PaymentMethodStatus.Debit).Sum(so => so.DebitMoney))
            .Take(5)
            .ToListAsync();
    }

    public async Task<NewCustomerStatsDto> GetNewCustomerStats()
    {
        var now = _dateTimeProvider.UtcNow.AddHours(7);

        var todayStart = now.Date.AddHours(-7);
        var todayEnd = todayStart.AddDays(1);
        var yesterdayStart = todayStart.AddDays(-1);
        var yesterdayEnd = todayStart;

        var todayCount = await _context.Customers
            .CountAsync(c => c.CreatedAt >= todayStart && c.CreatedAt < todayEnd);

        var yesterdayCount = await _context.Customers
            .CountAsync(c => c.CreatedAt >= yesterdayStart && c.CreatedAt < yesterdayEnd);

        return new NewCustomerStatsDto
        {
            TodayCount = todayCount,
            YesterdayCount = yesterdayCount
        };
    }
}