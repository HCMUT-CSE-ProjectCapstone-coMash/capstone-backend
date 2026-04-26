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
                    request.Combos[i].Items[j].Quantity * request.Combos[i].Quantity,
                    request.Combos[i].ComboDealId
                );
            }
        }

        await _saleOrdersService.UpdateTotalPriceAndTotalProfit(saleOrderId.Value);

        var result = await _saleOrdersService.GetSaleOrderById(saleOrderId.Value);

        return Ok(result.Value);
    }

    [HttpGet("fetch-all")]
    public async Task<IActionResult> FetchAllSaleOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _saleOrdersService.FetchAllSaleOrders(page, pageSize, search);

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
}