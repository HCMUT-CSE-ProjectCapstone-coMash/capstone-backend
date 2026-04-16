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
            .Where(c => EF.Functions.ILike(c.CustomerPhoneNumber, phonePattern))
            .Take(8)
            .ToListAsync();
    }

    public async Task<List<Customer>> FetchAllCustomers()
    {
        return await _context.Customers
            .Include(c => c.SaleOrders)
            .ToListAsync();
    }
}