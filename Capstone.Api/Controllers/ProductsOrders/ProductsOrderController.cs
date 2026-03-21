using Capstone.Application.Services.ProductsOrders;
using Capstone.Contracts.ProductsOrders;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.ProductsOrders;

[ApiController]
[Route("products-orders")]
public class ProductsOrdersController : ControllerBase
{
    private readonly IProductsOrdersService _productsOrdersService;

    public ProductsOrdersController(IProductsOrdersService productsOrdersService)
    {
        _productsOrdersService = productsOrdersService;
    }

    [HttpPost("fetch/{createdBy}")]
    public async Task<IActionResult> FetchOrCreateProductsOrders([FromRoute] string createdBy)
    {
        var result = await _productsOrdersService.FetchOrCreateProductsOrders(createdBy);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new ProductsOrdersResponse(
            result.Value.Id,
            result.Value.CreatedBy,
            result.Value.CreatedAt,
            result.Value.OrderName,
            result.Value.OrderDescription,
            result.Value.OrderStatus,
            result.Value.Products.Select(p => new ProductResponse(
                p.Id,
                p.ProductId,
                p.ProductName,
                p.Category,
                p.Color,
                p.Pattern,
                p.SizeType,
                p.Quantities.Select(q => new ProductQuantityResponse(q.Size, q.Quantities)).ToList(),
                p.CreatedBy,
                p.CreatedAt,
                p.Status,
                p.ImageURL,
                p.VectorId
            )).ToList()
        ));
    }

    [HttpDelete("delete/{orderId}/{productId}")]
    public async Task<IActionResult> DeleteProductFromProductsOrders([FromRoute] string orderId, [FromRoute] string productId)
    {
        var result = await _productsOrdersService.DeleteProductFromProductsOrders(orderId, productId);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new DeleteProductFromProductsOrdersResponse(
            result.Value
        ));
    }
}