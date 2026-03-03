using Capstone.Application.Services.Products;
using Capstone.Contracts.Products;
using Capstone.Domain.Entities;
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

    // [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        var result = await _productsSerivce.CreateProduct(
            request.ProductID,
            request.ProductName,
            request.Category,
            request.Color,
            request.Pattern,
            request.SizeType,
            request.Quantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            request.CreatedBy
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
            result.Value.ProductID,
            result.Value.ProductName,
            result.Value.Category,
            result.Value.Color,
            result.Value.Pattern,
            result.Value.SizeType,
            result.Value.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
            result.Value.CreatedBy,
            result.Value.CreatedAt,
            result.Value.Status
        ));
    }

    [Authorize]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingProductsByUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var result = await _productsSerivce.GetPendingProductsByUserId(Guid.Parse(userId!));

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value.Select(product => new ProductResponse(
            product.Id,
            product.ProductID,
            product.ProductName,
            product.Category,
            product.Color,
            product.Pattern,
            product.SizeType,
            product.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
            product.CreatedBy,
            product.CreatedAt,
            product.Status
        )).ToList());
    }

    [HttpPatch("update/{productId}")]
    public async Task<IActionResult> UpdateProductStatus([FromBody] PatchProductRequest request, [FromRoute] string productId)
    {
        return Ok(request);
    }
}