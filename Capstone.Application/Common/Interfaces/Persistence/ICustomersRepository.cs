using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface ICustomersRepository
{
    Task CreateCustomer(Customer customer);
    Task<Customer?> GetCustomerByPhone(string customerPhone);
    Task<List<Customer>> FetchCustomerByName(string customerName);
    Task<List<Customer>> FetchCustomerByPhone(string customerPhone);
    Task<(List<Customer> Items, int Total)> FetchCustomers(int page, int pageSize, string? search = null, bool onlyDebt = false);
    Task<Customer?> GetCustomerById(Guid customerId);
    Task<List<Customer>> FetchTop5DebtCustomers();
    Task<NewCustomerStatsDto> GetNewCustomerStats();
}

public class NewCustomerStatsDto
{
    public int TodayCount { get; set; }
    public int YesterdayCount { get; set; }
}