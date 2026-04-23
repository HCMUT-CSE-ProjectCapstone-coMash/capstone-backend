using Capstone.Application.Services.ComboPromotionsService;
using Capstone.Application.Services.OrderPromotionsService;
using Capstone.Application.Services.ProductPromotionsService;
using Capstone.Application.Services.Promotions;
using Capstone.Contracts.Promotions;
using Capstone.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Promotions;

[ApiController]
[Route("promotions")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionsService _promotionsService;
    private readonly IProductPromotionsService _productPromotionsService;
    private readonly IOrderPromotionsService _orderPromotionsService;
    private readonly IComboPromotionsService _comboPromotionsService;

    public PromotionsController(
        IPromotionsService promotionsService,
        IProductPromotionsService productPromotionsService,
        IOrderPromotionsService orderPromotionsService,
        IComboPromotionsService comboPromotionsService
    )
    {
        _promotionsService = promotionsService;
        _productPromotionsService = productPromotionsService;
        _orderPromotionsService = orderPromotionsService;
        _comboPromotionsService = comboPromotionsService;
    }

    [HttpGet("create-promotion-id")]
    public async Task<IActionResult> CreatePromotionId()
    {
        var result = await _promotionsService.CreatePromotionId();

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new CreatePromotionIdResponse
        {
            PromotionId = result.Value
        });
    }

    [HttpPost("create/{createdBy}")]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request, string createdBy)
    {
        string promotionType;

        switch (request)
        {
            case CreateProductPromotionRequest:
                promotionType = PromotionType.ProductPromotion;
                break;
            case CreateComboPromotionRequest:
                promotionType = PromotionType.ComboPromotion;
                break;
            case CreateOrderPromotionRequest:
                promotionType = PromotionType.OrderPromotion;
                break;
            default:
                return BadRequest(new
                {
                    error = "InvalidPromotionType",
                    message = "The promotion type is invalid."
                });
        }

        var promotionId = await _promotionsService.CreatePromotion(
            request.PromotionName,
            promotionType,
            request.Description ?? string.Empty,
            request.StartDate,
            request.EndDate,
            createdBy
        );

        if (request is CreateProductPromotionRequest productPromotionRequest)
        {
            foreach (var productPromotion in productPromotionRequest.ProductDiscounts)
            {
                await _productPromotionsService.CreateProductPromotion(
                    promotionId.Value,
                    productPromotion.ProductId,
                    productPromotion.DiscountType,
                    productPromotion.DiscountValue
                );
            }
        }

        if (request is CreateComboPromotionRequest comboPromotionRequest)
        {
            foreach (var comboPromotion in comboPromotionRequest.Combos)
            {
                var comboPromotionId = await _comboPromotionsService.CreateComboPromotion(
                    promotionId.Value,
                    comboPromotion.ComboName,
                    comboPromotion.ComboPrice
                );

                foreach (var comboItem in comboPromotion.ComboItems)
                {
                    await _comboPromotionsService.CreateComboPromotionDetail(
                        comboPromotionId.Value,
                        comboItem.ProductId,
                        comboItem.Quantity
                    );
                }
            }
        }

        if (request is CreateOrderPromotionRequest orderPromotionRequest)
        {
            foreach (var orderPromotion in orderPromotionRequest.Levels)
            {
                await _orderPromotionsService.CreateOrderPromotion(
                    promotionId.Value,
                    orderPromotion.MinValue,
                    orderPromotion.DiscountType,
                    orderPromotion.DiscountValue,
                    orderPromotion.MaxDiscount ?? 0
                );
            }
        }

        return Ok(new { message = "Promotion created successfully", promotionName = request.PromotionName });
    }

    [HttpGet("fetch-all")]
    public async Task<IActionResult> FetchPromotions(
        [FromQuery] int currentPage = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null
    )
    {
        var result = await _promotionsService.FetchPromotions(currentPage, pageSize, category, search);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new GetPromotionResponse(
            result.Value.Items.Select(p => new PromotionResponse(
                p.Id,
                p.PromotionId,
                p.PromotionName,
                p.PromotionType,
                p.Description,
                p.PromotionStatus,
                p.PromotionPhase,
                p.StartDate,
                p.EndDate,
                p.CreatedAt
            )).ToList(),
            result.Value.Total
        ));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPromotionById([FromRoute] string id)
    {
        var promotion = await _promotionsService.GetPromotionById(id);

        if (promotion.IsFailure)
        {
            return BadRequest(new
            {
                error = promotion.Error.Code,
                message = promotion.Error.Description
            });
        }

        switch (promotion.Value.PromotionType)
        {
            case PromotionType.ProductPromotion:
                var productPromotions = await _productPromotionsService.GetProductPromotionsByPromotionId(promotion.Value.Id);
                promotion.Value.ProductDiscounts = productPromotions.Value;
                break;
            case PromotionType.ComboPromotion:
                var comboPromotions = await _comboPromotionsService.GetComboPromotionsByPromotionId(promotion.Value.Id);
                promotion.Value.Combos = comboPromotions.Value;
                break;
            case PromotionType.OrderPromotion:
                var orderPromotions = await _orderPromotionsService.GetOrderPromotionsByPromotionId(promotion.Value.Id);
                promotion.Value.Levels = orderPromotions.Value;
                break;
        }

        return Ok(promotion.Value);
    }

    [HttpPatch("product/{promotionId}")]
    public async Task<IActionResult> UpdateProductPromotion(string promotionId, [FromBody] UpdateProductPromotionRequest request)
    {
        var promotionName = await _promotionsService.UpdatePromotion(
            promotionId,
            request.PromotionName,
            request.StartDate,
            request.EndDate,
            request.Description ?? string.Empty
        );

        if (promotionName.IsFailure)
        {
            return BadRequest(new
            {
                error = promotionName.Error.Code,
                message = promotionName.Error.Description
            });
        }

        await _productPromotionsService.DeleteProductPromotionsByPromotionId(Guid.Parse(promotionId));

        foreach (var productPromotion in request.ProductDiscounts)
        {
            await _productPromotionsService.CreateProductPromotion(
                promotionId,
                productPromotion.ProductId,
                productPromotion.DiscountType,
                productPromotion.DiscountValue
            );
        }

        return Ok(new { message = "Promotion updated successfully", promotionName = promotionName.Value });
    }

    [HttpPatch("combo/{promotionId}")]
    public async Task<IActionResult> UpdateComboPromotion(string promotionId, [FromBody] UpdateComboPromotionRequest request)
    {
        var promotionName = await _promotionsService.UpdatePromotion(
            promotionId,
            request.PromotionName,
            request.StartDate,
            request.EndDate,
            request.Description ?? string.Empty
        );

        if (promotionName.IsFailure)
        {
            return BadRequest(new
            {
                error = promotionName.Error.Code,
                message = promotionName.Error.Description
            });
        }

        await _comboPromotionsService.DeleteComboPromotionsByPromotionId(promotionId);

        foreach (var comboPromotion in request.Combos)
        {
            var comboPromotionId = await _comboPromotionsService.CreateComboPromotion(
                promotionId,
                comboPromotion.ComboName,
                comboPromotion.ComboPrice
            );

            foreach (var comboItem in comboPromotion.ComboItems)
            {
                await _comboPromotionsService.CreateComboPromotionDetail(
                    comboPromotionId.Value,
                    comboItem.ProductId,
                    comboItem.Quantity
                );
            }
        }

        return Ok(new { message = "Promotion updated successfully", promotionName = promotionName.Value });
    }

    [HttpPatch("order/{promotionId}")]
    public async Task<IActionResult> UpdateOrderPromotion(string promotionId, [FromBody] UpdateOrderPromotionRequest request)
    {
        var promotionName = await _promotionsService.UpdatePromotion(
            promotionId,
            request.PromotionName,
            request.StartDate,
            request.EndDate,
            request.Description ?? string.Empty
        );

        if (promotionName.IsFailure)
        {
            return BadRequest(new
            {
                error = promotionName.Error.Code,
                message = promotionName.Error.Description
            });
        }

        await _orderPromotionsService.DeleteOrderPromotionsByPromotionId(promotionId);

        foreach (var orderPromotion in request.Levels)
        {
            await _orderPromotionsService.CreateOrderPromotion(
                promotionId,
                orderPromotion.MinValue,
                orderPromotion.DiscountType,
                orderPromotion.DiscountValue,
                orderPromotion.MaxDiscount ?? 0
            );
        }

        return Ok(new { message = "Promotion updated successfully", promotionName = promotionName.Value });
    }

    [HttpGet("get-promotions/{productId}")]
    public async Task<IActionResult> GetPromotionsByProductId(string productId)
    {
        var productPromotions = await _promotionsService.GetProductPromotionsByProductId(productId);

        if (productPromotions.IsFailure)
        {
            return BadRequest(new
            {
                error = productPromotions.Error.Code,
                message = productPromotions.Error.Description
            });
        }

        var comboPromotions = await _promotionsService.GetComboPromotionsByProductId(productId);

        if (comboPromotions.IsFailure)
        {
            return BadRequest(new
            {
                error = comboPromotions.Error.Code,
                message = comboPromotions.Error.Description
            });
        }

        return Ok(new {
            ProductPromotions = productPromotions.Value,
            ComboPromotions = comboPromotions.Value
        });
    }
}