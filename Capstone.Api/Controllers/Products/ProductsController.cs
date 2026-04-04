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
            result.Value.VectorId,
            result.Value.SalePrice,
            result.Value.ImportPrice
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
            result.Value.VectorId,
            result.Value.SalePrice,
            result.Value.ImportPrice
        ));
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeImage([FromBody] AnalyzeImageRequest request)
    {
        var result = await _productsSerivce.AnalyzeImage(request.ImageBase64);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new AnalyzeImageResponse(
            result.Value.ProductId,
            result.Value.ProductName,
            result.Value.Category,
            result.Value.Color,
            result.Value.Pattern
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
            result.Value.VectorId,
            result.Value.SalePrice,
            result.Value.ImportPrice
        ));
    }

    [HttpGet("fetch-by-name/{productName}")]
    public async Task<IActionResult> FetchApprovedProductByName([FromRoute] string productName)
    {
        var result = await _productsSerivce.FetchApprovedProductByName(productName);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value.Select(p => new ProductResponse(
            p.Id,
            p.ProductId,
            p.ProductName,
            p.Category,
            p.Color,
            p.Pattern,
            p.SizeType,
            p.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
            p.CreatedBy,
            p.CreatedAt,
            p.Status,
            p.ImageURL,
            p.VectorId,
            p.SalePrice,
            p.ImportPrice
        )).ToList());
    }

    [HttpGet("create-product-id-by-category/{category}")]
    public async Task<IActionResult> CreateProductIdByCategory([FromRoute] string category)
    {
        var result = await _productsSerivce.CreateProductIdByCategory(category);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new CreateProductIdByCategoryResponse(
            result.Value
        ));
    }

    [HttpPost("create-detail-for-approved-product/{productId}/{productsOrderId}")]
    public async Task<IActionResult> CreateDetailForApprovedProduct(
        [FromForm] CreateDetailForApprovedProductRequest request,
        [FromRoute] string productId,
        [FromRoute] string productsOrderId
    )
    {
        var result = await _productsSerivce.CreateDetailForApprovedProduct(
            productId,
            productsOrderId,
            request.Quantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList()
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
            result.Value.Product.Id,
            result.Value.Product.ProductId,
            result.Value.Product.ProductName,
            result.Value.Product.Category,
            result.Value.Product.Color,
            result.Value.Product.Pattern,
            result.Value.Product.SizeType,
            result.Value.Product.Quantities.Select(q => new ProductQuantity(q.Size, q.Quantities)).ToList(),
            result.Value.Product.CreatedBy,
            result.Value.Product.CreatedAt,
            result.Value.Product.Status,
            result.Value.Product.ImageURL,
            result.Value.Product.VectorId,
            result.Value.Product.SalePrice,
            result.Value.Product.ImportPrice,
            result.Value.QuantityChanges.Select(qc => new ProductQuantityChange(qc.Size, qc.OldQuantity, qc.NewQuantity)).ToList()
        ));
    }
}