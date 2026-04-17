using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class CustomersRepository : ICustomersRepository
{
    private readonly AppDbContext _context;

    public CustomersRepository(AppDbContext context)
    {
        _context = context;
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

    public async Task<(List<Customer> Items, int Total)> FetchCustomers(int page, int pageSize, string? search = null)
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";
            query = query.Where(c => EF.Functions.ILike(c.CustomerName, searchPattern)
                || EF.Functions.ILike(c.CustomerPhoneNumber, searchPattern));
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
}