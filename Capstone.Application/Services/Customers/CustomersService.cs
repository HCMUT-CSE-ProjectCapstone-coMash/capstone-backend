using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.Customers;

public class CustomersService : ICustomersService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICustomersRepository _customers;

    public CustomersService(IDateTimeProvider dateTimeProvider, ICustomersRepository customersRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _customers = customersRepository;
    }

    public async Task<Result<List<CustomerDto>>> FetchCustomerByName(string customerName)
    {
        var customer = await _customers.FetchCustomerByName(customerName);

        var customerDtos = customer.Select(c => new CustomerDto(
            c.Id,
            c.CustomerName,
            c.CustomerPhoneNumber,
            c.CustomerStatus,
            c.CreatedAt,
            c.CreatedBy
        )).ToList();

        return Result<List<CustomerDto>>.Success(customerDtos);
    }

    public async Task<Result<List<CustomerDto>>> FetchCustomerByPhone(string customerPhone)
    {
        var customer = await _customers.FetchCustomerByPhone(customerPhone);

        var customerDtos = customer.Select(c => new CustomerDto(
            c.Id,
            c.CustomerName,
            c.CustomerPhoneNumber,
            c.CustomerStatus,
            c.CreatedAt,
            c.CreatedBy
        )).ToList();

        return Result<List<CustomerDto>>.Success(customerDtos);
    }

    public async Task<Result<CustomerDto>> CreateCustomer(string customerName, string customerPhone, string userId)
    {
        var customer = await _customers.GetCustomerByPhone(customerPhone);

        if (customer != null)
        {
            return Result<CustomerDto>.Failure(new Error("CustomerAlreadyExists", "Số điện thoại đã tồn tại"));
        }

        var newCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            CustomerPhoneNumber = customerPhone,
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedBy = Guid.Parse(userId),
            CustomerStatus = CustomersStatus.Active
        };

        await _customers.CreateCustomer(newCustomer);

        return Result<CustomerDto>.Success(new CustomerDto(
            newCustomer.Id,
            newCustomer.CustomerName,
            newCustomer.CustomerPhoneNumber,
            newCustomer.CustomerStatus,
            newCustomer.CreatedAt,
            newCustomer.CreatedBy
        ));
    }
}