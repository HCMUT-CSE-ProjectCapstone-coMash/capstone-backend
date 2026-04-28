using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ICustomersRepository
{
    Task CreateCustomer(Customer customer);
    Task<Customer?> GetCustomerByPhone(string customerPhone);
    Task<List<Customer>> FetchCustomerByName(string customerName);
    Task<List<Customer>> FetchCustomerByPhone(string customerPhone);
    Task<(List<Customer> Items, int Total)> FetchCustomers(int page, int pageSize, string? search = null);
    Task<Customer?> GetCustomerById(Guid customerId);
}

