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

        var customerDtos = customer.Select(c =>
        {
            var debitOrders = c.SaleOrders
                .Where(so => so.PaymentMethod == PaymentMethodStatus.Debit)
                .ToList();

            var totalDebit = debitOrders.Sum(so => so.DebitMoney);
            var debitDays = 0;

            if (debitOrders.Any())
            {
                var earliestDebitAt = debitOrders.Min(so => so.CreatedAt);
                debitDays = (int)Math.Max(0, (_dateTimeProvider.UtcNow - earliestDebitAt).TotalDays);
            }

            return new CustomerDto(
                c.Id,
                c.CustomerName,
                c.CustomerPhoneNumber,
                c.CustomerStatus,
                c.CreatedAt,
                totalDebit,
                debitDays
            );
        }).ToList();

        return Result<List<CustomerDto>>.Success(customerDtos);
    }

    public async Task<Result<List<CustomerDto>>> FetchCustomerByPhone(string customerPhone)
    {
        var customer = await _customers.FetchCustomerByPhone(customerPhone);

        var customerDtos = customer.Select(c =>
        {
            var debitOrders = c.SaleOrders
                .Where(so => so.PaymentMethod == PaymentMethodStatus.Debit)
                .ToList();

            var totalDebit = debitOrders.Sum(so => so.DebitMoney);
            var debitDays = 0;

            if (debitOrders.Any())
            {
                var earliestDebitAt = debitOrders.Min(so => so.CreatedAt);
                debitDays = (int)Math.Max(0, (_dateTimeProvider.UtcNow - earliestDebitAt).TotalDays);
            }

            return new CustomerDto(
                c.Id,
                c.CustomerName,
                c.CustomerPhoneNumber,
                c.CustomerStatus,
                c.CreatedAt,
                totalDebit,
                debitDays
            );
        }).ToList();

        return Result<List<CustomerDto>>.Success(customerDtos);
    }

    public async Task<Result<PaginatedResult<CustomerDto>>> FetchAllCustomers(int page, int pageSize, string? search = null)
    {
        var (customers, total) = await _customers.FetchCustomers(page, pageSize, search);

        var customerDtos = customers.Select(c =>
        {
            var debitOrders = c.SaleOrders
                .Where(so => so.PaymentMethod == PaymentMethodStatus.Debit)
                .ToList();

            var totalDebit = debitOrders.Sum(so => so.DebitMoney);
            var debitDays = 0;

            if (debitOrders.Any())
            {
                var earliestDebitAt = debitOrders.Min(so => so.CreatedAt);
                debitDays = (int)Math.Max(0, (_dateTimeProvider.UtcNow - earliestDebitAt).TotalDays);
            }

            return new CustomerDto(
                c.Id,
                c.CustomerName,
                c.CustomerPhoneNumber,
                c.CustomerStatus,
                c.CreatedAt,
                totalDebit,
                debitDays
            );
        }).ToList();

        return Result<PaginatedResult<CustomerDto>>.Success(
            new PaginatedResult<CustomerDto>(customerDtos, total));
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
            0,
            0
        ));
    }

    public async Task<Result<CustomerDto>> FetchCustomerById(string customerId)
    {
        var customer = await _customers.GetCustomerById(Guid.Parse(customerId));

        if (customer == null)
        {
            return Result<CustomerDto>.Failure(new Error("CustomerNotFound", "Khách hàng không tồn tại"));
        }

        var debitOrders = customer.SaleOrders
            .Where(so => so.PaymentMethod == PaymentMethodStatus.Debit)
            .ToList();

        var totalDebit = debitOrders.Sum(so => so.DebitMoney);
        var debitDays = 0;

        if (debitOrders.Any())
        {
            var earliestDebitAt = debitOrders.Min(so => so.CreatedAt);
            debitDays = (int)Math.Max(0, (_dateTimeProvider.UtcNow - earliestDebitAt).TotalDays);
        }

        return Result<CustomerDto>.Success(new CustomerDto(
            customer.Id,
            customer.CustomerName,
            customer.CustomerPhoneNumber,
            customer.CustomerStatus,
            customer.CreatedAt,
            totalDebit,
            debitDays
        ));
    }
}