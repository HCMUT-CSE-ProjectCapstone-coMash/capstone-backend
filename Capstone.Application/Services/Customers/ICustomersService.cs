using Capstone.Application.Common;

namespace Capstone.Application.Services.Customers;

public interface ICustomersService
{
    Task<Result<List<CustomerDto>>> FetchCustomerByName(string customerName);
    Task<Result<List<CustomerDto>>> FetchCustomerByPhone(string customerPhone);
    Task<Result<PaginatedResult<CustomerDto>>> FetchAllCustomers(int page, int pageSize, string? search = null, bool onlyDebt = false);
    Task<Result<CustomerDto>> CreateCustomer(string customerName, string customerPhone, string userId);
    Task<Result<CustomerDto>> FetchCustomerById(string customerId);
    Task<Result<List<CustomerDto>>> FetchTop5DebtCustomers();
    Task<Result<NewCustomerStatsDto>> GetNewCustomerStats();
}