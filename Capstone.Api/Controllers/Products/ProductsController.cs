using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.ProductQuantitesService;
using Capstone.Application.Services.Products;
using Capstone.Application.Services.ProductsOrdersDetailService;
using Capstone.Contracts.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Products;

[ApiController]
[Route("product")]
public class ProductsController : ControllerBase
{
    private readonly IProductsService _productsSerivce;
    private readonly IProductQuantitiesService _productQuantitiesService;
    private readonly IProductsOrdersDetailService _productsOrdersDetailService;

    private readonly IFileStorageService _fileStorageService;

    public ProductsController(
        IProductsService productsSerivce,
        IProductQuantitiesService productQuantitiesService,
        IProductsOrdersDetailService productsOrdersDetailService,
        IFileStorageService fileStorageService
    )
    {
        _productsSerivce = productsSerivce;
        _productQuantitiesService = productQuantitiesService;
        _productsOrdersDetailService = productsOrdersDetailService;
        _fileStorageService = fileStorageService;
    }

    [Authorize]
    [HttpPost("create/{orderId}")]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request, [FromRoute] string orderId)
    {
        var productResult = await _productsSerivce.CreateProduct(
            request.ProductName,
            request.Category,
            request.Color,
            request.Pattern ?? string.Empty,
            request.SizeType,
            request.CreatedBy
        );

        for (int i = 0; i < request.Quantities.Count; i++)
        {
            var quantity = request.Quantities[i];
            await _productQuantitiesService.CreateProductQuantities(productResult.Value, quantity.Size, quantity.Quantities);
        }

        await _productsOrdersDetailService.CreateProductsOrdersDetail(orderId, productResult.Value);

        if (request.Image != null)
        {
            var extension = Path.GetExtension(request.Image.FileName);

            var ImageResult = await _fileStorageService.UploadImageAsync(
                "products",
                productResult.Value,
                request.Image.OpenReadStream(),
                request.Image.ContentType,
                extension
            );

            await _productsSerivce.UpdateProductImageKey(productResult.Value, ImageResult.Value);
        }

        var result = await _productsSerivce.FetchProductById(productResult.Value);

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

        return Ok(result.Value.Select(p => new ProductWithOrderStatusResponse(
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
            p.ImportPrice,
            p.IsInPendingOrder
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

    [HttpPost("owner-create")]
    public async Task<IActionResult> OwnerCreateProduct([FromForm] OwnerCreateProductRequest request)
    {
        var productResult = await _productsSerivce.OwnerCreateProduct(
            request.ProductName,
            request.Category,
            request.Color,
            request.Pattern ?? string.Empty,
            request.SizeType,
            request.CreatedBy,
            request.SalePrice,
            request.ImportPrice
        );

        for (int i = 0; i < request.Quantities.Count; i++)
        {
            var quantity = request.Quantities[i];
            await _productQuantitiesService.CreateProductQuantities(productResult.Value, quantity.Size, quantity.Quantities);
        }

        if (request.Image != null)
        {
            var extension = Path.GetExtension(request.Image.FileName);

            var ImageResult = await _fileStorageService.UploadImageAsync(
                "products",
                productResult.Value,
                request.Image.OpenReadStream(),
                request.Image.ContentType,
                extension
            );

            await _productsSerivce.UpdateProductImageKey(productResult.Value, ImageResult.Value);
        }

        var result = await _productsSerivce.FetchProductById(productResult.Value);

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

    [HttpPatch("owner-patch/{productId}")]
    public async Task<IActionResult> OwnerPatchProduct([FromForm] OwnerPatchProductRequest request, [FromRoute] string productId)
    {
        var result = await _productsSerivce.OwnerPatchProduct(
            productId,
            request.ProductId,
            request.ProductName,
            request.Category,
            request.Color,
            request.Pattern,
            request.SizeType,
            request.Quantities?.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            request.SalePrice,
            request.ImportPrice
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

    [HttpGet("fetch-all")]
    public async Task<IActionResult> FetchAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? category = null, [FromQuery] string? search = null)
    {
        var result = await _productsSerivce.FetchAllProducts(page, pageSize, category, search);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new GetProductsResponse(
            result.Value.Items.Select(p => new ProductResponse(
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
            )).ToList(),
            result.Value.Total
        ));
    }

    [HttpPatch("owner-patch-in-products-order/{productId}/{productsOrderId}")]
    public async Task<IActionResult> OwnerUpdateProductInProductsOrder([FromForm] OwnerPatchInProductsOrderRequest request, [FromRoute] string productId, [FromRoute] string productsOrderId)
    {
        var result = await _productsSerivce.OwnerUpdateProductInProductsOrder(
            productId,
            productsOrderId,
            request.ProductName,
            request.Color,
            request.Pattern,
            request.SizeType,
            request.Quantities?.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
            request.SalePrice,
            request.ImportPrice
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

    [HttpPatch("employee-patch-in-products-order/{productId}/{productsOrderId}")]
    public async Task<IActionResult> EmployeeUpdateProductInProductsOrder([FromForm] EmployeePatchInProductsOrderRequest request, [FromRoute] string productId, [FromRoute] string productsOrderId)
    {
        var result = await _productsSerivce.EmployeeUpdateProductInProductsOrder(
            productId,
            productsOrderId,
            request.ProductName,
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
            0,
            result.Value.QuantityChanges.Select(qc => new ProductQuantityChange(qc.Size, qc.OldQuantity, qc.NewQuantity)).ToList()
        ));
    }
}