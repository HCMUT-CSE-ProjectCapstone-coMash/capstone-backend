using Capstone.Application.Services.SaleOrderDetails;
using Capstone.Application.Services.SaleOrders;
using Capstone.Contracts.SaleOrders;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.SaleOrders;

[ApiController]
[Route("sale-orders")]
public class SaleOrdersController : ControllerBase
{
    private readonly ISaleOrdersService _saleOrdersService;
    private readonly ISaleOrderDetailsService _saleOrderDetailsService;

    public SaleOrdersController(
        ISaleOrdersService saleOrdersService,
        ISaleOrderDetailsService saleOrderDetailsService
    )
    {
        _saleOrdersService = saleOrdersService;
        _saleOrderDetailsService = saleOrderDetailsService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSaleOrders([FromBody] CreateSaleOrdersRequest request)
    {
        var saleOrderId = await _saleOrdersService.CreateSaleOrder(
            request.CustomerId,
            request.UserId,
            request.PaymentMethod,
            request.DebtAmount
        );

        for (int i = 0; i < request.Products.Count; i++)
        {
            await _saleOrderDetailsService.CreateSaleOrderDetailForProductPromotion(
               saleOrderId.Value,
               request.Products[i].ProductId,
               request.Products[i].SelectedSize,
               request.Products[i].Quantity,
               request.Products[i].Discount,
               request.Products[i].PromotionId
           );
        }

        for (int i = 0; i < request.Combos.Count; i++)
        {
            for (int j = 0; j < request.Combos[i].Items.Count; j++)
            {
                await _saleOrderDetailsService.CreateSaleOrderDetailForComboPromotion(
                    saleOrderId.Value,
                    request.Combos[i].Items[j].ProductId,
                    request.Combos[i].Items[j].SelectedSize,
                    request.Combos[i].Items[j].Quantity,
                    request.Combos[i].ComboDealId
                );
            }
        }

        await _saleOrdersService.UpdateTotalPriceAndTotalProfit(saleOrderId.Value, request.OrderPromotionId);

        var result = await _saleOrdersService.GetSaleOrderById(saleOrderId.Value);

        return Ok(result.Value);
    }

    [HttpGet("fetch-all")]
    public async Task<IActionResult> FetchAllSaleOrders([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10, [FromQuery] string? timeRange = null, [FromQuery] string? search = null)
    {
        var result = await _saleOrdersService.FetchAllSaleOrders(currentPage, pageSize, timeRange, search);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("{saleOrderId}")]
    public async Task<IActionResult> GetSaleOrderById([FromRoute] string saleOrderId)
    {
        var result = await _saleOrdersService.GetSaleOrderById(saleOrderId);

        if (result.IsFailure)
        {
            return NotFound(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("get-sale-orders-with-debt/{customerId}")]
    public async Task<IActionResult> GetAllSaleOrdersWithDebt([FromRoute] string customerId)
    {
        var result = await _saleOrdersService.GetAllSaleOrdersWithDebt(customerId);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("fetch-all-by-employee/{employeeId}")]
    public async Task<IActionResult> FetchAllSaleOrdersByEmployeeId([FromRoute] string employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _saleOrdersService.FetchAllSaleOrdersByEmployeeId(employeeId, page, pageSize, search);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("fetch-all-by-customer/{customerId}")]
    public async Task<IActionResult> FetchAllSaleOrdersByCustomerId([FromRoute] string customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _saleOrdersService.FetchAllSaleOrdersByCustomerId(customerId, page, pageSize, search);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("pay-debt/{customerId}")]
    public async Task<IActionResult> PayDebt([FromRoute] string customerId, [FromBody] PayDebtRequest request)
    {
        var result = await _saleOrdersService.PayDebt(customerId, request.PaymentAmount);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("income-stats")]
    public async Task<IActionResult> GetIncomeStats([FromQuery] string period = "day")
    {
        var result = await _saleOrdersService.GetIncomeStats(period);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("personal-income-stats/{userId}")]
    public async Task<IActionResult> GetPersonalIncomeStats([FromRoute] string userId, [FromQuery] string period = "day")
    {
        var result = await _saleOrdersService.GetPersonalIncomeStats(userId, period);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("top-customers-spending-stats")]
    public async Task<IActionResult> GetTopCustomersSpendingStats()
    {
        var result = await _saleOrdersService.GetTopCustomersSpendingStats(5);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await _saleOrdersService.GetDashboardStats();

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("dashboard-stats/employee/{employeeId}")]
    public async Task<IActionResult> FetchEmployeeDashboardStats([FromRoute] string employeeId)
    {
        var result = await _saleOrdersService.GetEmployeeDashboardStats(employeeId);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("fetch-recent-created-by-employee/{employeeId}")]
    public async Task<IActionResult> FetchRecentCreatedByEmployee([FromRoute] string employeeId)
    {
        var result = await _saleOrdersService.FetchRecentCreatedByEmployee(employeeId);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("create-payments")]
    public async Task<IActionResult> CreatePayments([FromBody] CreatePaymentRequest request)
    {
        var result = await _saleOrdersService.CreatePayments(
            request.OrderCode,
            request.Amount,
            request.Description,
            request.CancelUrl,
            request.ReturnUrl
        );

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }
}