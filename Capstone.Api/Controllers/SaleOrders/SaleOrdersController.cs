using Capstone.Application.Services.SaleOrders;
using Capstone.Contracts.SaleOrders;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.SaleOrders;

[ApiController]
[Route("sale-orders")]
public class SaleOrdersController : ControllerBase
{
    private readonly ISaleOrdersService _saleOrdersService;

    public SaleOrdersController(ISaleOrdersService saleOrdersService)
    {
        _saleOrdersService = saleOrdersService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSaleOrders([FromBody] CreateSaleOrdersRequest request)
    {
        var result = await _saleOrdersService.CreateSaleOrder(
            request.CustomerId,
            request.UserId,
            request.PaymentMethod,
            request.DebitMoney,
            request.Discount
        );


        return Ok(result);
    }
}