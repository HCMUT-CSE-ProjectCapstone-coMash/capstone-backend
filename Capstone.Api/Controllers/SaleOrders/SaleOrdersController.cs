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
        var saleOrderResponse = await _saleOrdersService.CreateSaleOrder(
            request.CustomerId,
            request.UserId,
            request.PaymentMethod,
            request.DebitMoney
        );

        for (int i = 0; i < request.Products.Count; i++)
        {
            var product = request.Products[i];
            await _saleOrderDetailsService.CreateSaleOrderDetail(
                saleOrderResponse.Value,
                product.ProductId,
                product.SelectedSize,
                product.Quantity,
                product.Discount
            );
        }

        await _saleOrdersService.UpdateTotalPrice(saleOrderResponse.Value);

        var result = await _saleOrdersService.GetSaleOrderById(saleOrderResponse.Value);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new SaleOrderResponse(
            result.Value.Id,
            result.Value.SaleOrderId,
            result.Value.CustomerId,
            result.Value.CustomerName,
            result.Value.CreatedBy,
            result.Value.CreatedByName,
            result.Value.PaymentMethod,
            result.Value.DebitMoney,
            result.Value.CreatedAt,
            result.Value.TotalPrice,
            result.Value.Details.Select(d => new SaleOrderDetailResponse(
                d.Id,
                d.ProductId,
                d.ProductName,
                d.SelectedSize,
                d.Quantity,
                d.UnitPrice,
                d.Discount,
                d.SubTotal
            )).ToList()
        ));
    }
}