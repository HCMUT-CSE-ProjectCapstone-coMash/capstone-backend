using Capstone.Application.Services.Products;
using Capstone.Contracts.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Products;

[ApiController]
[Route("product")]
public class ProductsController : ControllerBase
{
    private readonly IProductsService _productsSerivce;

    public ProductsController(IProductsService productsSerivce)
    {
        _productsSerivce = productsSerivce;
    }

    [Authorize]
    [HttpPost("create/{orderId}")]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request, [FromRoute] string orderId)
    {
        var result = await _productsSerivce.CreateProduct(
            request.ProductId,
            request.ProductName,
            request.Category,
            request.Color,
            request.Pattern,
            request.SizeType,
            request.Quantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            request.CreatedBy,
            request.Image,
            orderId
        );

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new ProductResponse(
            result.Value.Id,
            result.Value.ProductId,
            result.Value.ProductName,
            result.Value.Category,
            result.Value.Color,
            result.Value.Pattern,
            result.Value.SizeType,
            result.Value.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
            result.Value.CreatedBy,
            result.Value.CreatedAt,
            result.Value.Status,
            result.Value.ImageURL,
            result.Value.VectorId
        ));
    }

    [Authorize]
    [HttpPost("similar")]
    public async Task<IActionResult> SearchProductSimilar([FromBody] SearchProductSimilarRequest request)
    {
        var result = await _productsSerivce.SearchProductSimilar(request.ImageBase64);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }
        
        return Ok(new ProductResponse(
            result.Value.Id,
            result.Value.ProductId,
            result.Value.ProductName,
            result.Value.Category,
            result.Value.Color,
            result.Value.Pattern,
            result.Value.SizeType,
            result.Value.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
            result.Value.CreatedBy,
            result.Value.CreatedAt,
            result.Value.Status,
            result.Value.ImageURL,
            result.Value.VectorId
        ));
    }

    [HttpPatch("patch/{productId}")]
    public async Task<IActionResult> PatchProductInProductsOrders([FromBody] PatchProductRequest request, [FromRoute] string productId)
    {
        var result = await _productsSerivce.PatchProductInProductsOrders(
            productId,
            request.ProductID,
            request.ProductName,
            request.Category,
            request.Color,
            request.Pattern,
            request.SizeType,
            request.Quantities?.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList()
        );

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new ProductResponse(
            result.Value.Id,
            result.Value.ProductId,
            result.Value.ProductName,
            result.Value.Category,
            result.Value.Color,
            result.Value.Pattern,
            result.Value.SizeType,
            result.Value.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
            result.Value.CreatedBy,
            result.Value.CreatedAt,
            result.Value.Status,
            result.Value.ImageURL,
            result.Value.VectorId
        ));
    }
}