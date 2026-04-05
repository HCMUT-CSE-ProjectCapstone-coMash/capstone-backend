using Capstone.Application.Services.ProductsOrders;
using Capstone.Contracts.Products;
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

    [HttpGet("fetch-excluding-pending")]
    public async Task<IActionResult> GetAllProductsOrdersExcludingPending()
    {
        var result = await _productsOrdersService.GetAllProductsOrdersExcludingPending();

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value.Select(order => new ProductsOrdersListResponse(
            order.Id,
            order.CreatedBy,
            order.CreatedByName,
            order.CreatedAt,
            order.OrderName,
            order.OrderDescription,
            order.OrderStatus
        )));
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
                p.Product.Id,
                p.Product.ProductId,
                p.Product.ProductName,
                p.Product.Category,
                p.Product.Color,
                p.Product.Pattern,
                p.Product.SizeType,
                p.Product.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
                p.Product.CreatedBy,
                p.Product.CreatedAt,
                p.Product.Status,
                p.Product.ImageURL,
                p.Product.VectorId,
                null,
                null,
                p.QuantityChanges.Select(qc => new ProductQuantityChange(qc.Size, qc.OldQuantity, qc.NewQuantity)).ToList()
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

        return Ok(new DeleteProductFromProductsOrdersResponse(result.Value));
    }

    // Nhân viên gửi đơn hàng cho chủ cửa hàng duyệt
    [HttpPatch("patch/{orderId}")]
    public async Task<IActionResult> PatchProductsOrders([FromRoute] string orderId, [FromBody] PatchProductsOrders request)
    {
        var result = await _productsOrdersService.PatchProductsOrders(orderId, request.OrderName, request.OrderDescription, request.OrderStatus);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new PatchProductsOrdersResponse(result.Value));
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetProductsOrderById([FromRoute] string orderId)
    {
        var result = await _productsOrdersService.GetProductsOrderById(orderId);

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
                p.Product.Id,
                p.Product.ProductId,
                p.Product.ProductName,
                p.Product.Category,
                p.Product.Color,
                p.Product.Pattern,
                p.Product.SizeType,
                p.Product.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
                p.Product.CreatedBy,
                p.Product.CreatedAt,
                p.Product.Status,
                p.Product.ImageURL,
                p.Product.VectorId,
                null,
                null,
                p.QuantityChanges.Select(qc => new ProductQuantityChange(qc.Size, qc.OldQuantity, qc.NewQuantity)).ToList()
            )).ToList()
        ));
    }
}