using Capstone.Application.Services.Products;
using Capstone.Contracts.Products;
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
            request.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
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

        return Ok(new CreateProductResponse(
            result.Value.Id,
            result.Value.ProductID,
            result.Value.ProductName,
            result.Value.Category,
            result.Value.Color,
            result.Value.Pattern,
            result.Value.SizeType,
            result.Value.Quantities.Select(q => new ProductQuantityDTO(q.Size, q.Quantities)).ToList(),
            result.Value.CreatedBy,
            result.Value.CreatedAt,
            result.Value.Status
        ));
    }
}