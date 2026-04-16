using Capstone.Application.Services.Customers;
using Capstone.Contracts.Customers;
using Capstone.Contracts.SaleOrders;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Customers;

[ApiController]
[Route("customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomersService _customersService;

    public CustomersController(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    [HttpGet("fetch-by-name")]
    public async Task<IActionResult> FetchCustomerByName([FromQuery] string customerName)
    {
        var result = await _customersService.FetchCustomerByName(customerName);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        var customersResponse = result.Value.Select(c => new CustomersResponse(
            c.Id,
            c.CustomerName,
            c.CustomerPhone,
            c.CustomerStatus,
            c.CreatedAt,
            c.DebitMoney,
            c.DebitDays
        )).ToList();

        return Ok(customersResponse);
    }

    [HttpGet("fetch-by-phone")]
    public async Task<IActionResult> FetchCustomerByPhone([FromQuery] string customerPhone)
    {
        var result = await _customersService.FetchCustomerByPhone(customerPhone);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        var customersResponse = result.Value.Select(c => new CustomersResponse(
            c.Id,
            c.CustomerName,
            c.CustomerPhone,
            c.CustomerStatus,
            c.CreatedAt,
            c.DebitMoney,
            c.DebitDays
        )).ToList();

        return Ok(customersResponse);
    }

    [HttpGet("fetch-all")]
    public async Task<IActionResult> FetchAllCustomers()
    {
        var result = await _customersService.FetchAllCustomers();

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        var customersResponse = result.Value.Select(c => new CustomersResponse(
            c.Id,
            c.CustomerName,
            c.CustomerPhone,
            c.CustomerStatus,
            c.CreatedAt,
            c.DebitMoney,
            c.DebitDays
        )).ToList();

        return Ok(customersResponse);
    }

    [HttpPost("create/{userId}")]
    public async Task<IActionResult> CreateCustomer([FromRoute] string userId, [FromBody] CreateCustomersRequest request)
    {
        var result = await _customersService.CreateCustomer(request.CustomerName, request.customerPhone, userId);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new CustomersResponse(
            result.Value.Id,
            result.Value.CustomerName,
            result.Value.CustomerPhone,
            result.Value.CustomerStatus,
            result.Value.CreatedAt,
            result.Value.DebitMoney,
            result.Value.DebitDays
        ));
    }
}