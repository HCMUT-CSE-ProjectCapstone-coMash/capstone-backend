using Capstone.Application.Common;

namespace Capstone.Application.Services.Customers;

public interface ICustomersService
{
    Task<Result<List<CustomerDto>>> FetchCustomerByName(string customerName);
    Task<Result<List<CustomerDto>>> FetchCustomerByPhone(string customerPhone);
    Task<Result<CustomerDto>> CreateCustomer(string customerName, string customerPhone, string userId);
}