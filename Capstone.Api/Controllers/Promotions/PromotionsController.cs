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

    public PromotionsController(
        IPromotionsService promotionsService,
        IProductPromotionsService productPromotionsService,
        IOrderPromotionsService orderPromotionsService
    )
    {
        _promotionsService = promotionsService;
        _productPromotionsService = productPromotionsService;
        _orderPromotionsService = orderPromotionsService;
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

        var promotionResult = await _promotionsService.CreatePromotion(
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
                    promotionResult.Value,
                    productPromotion.ProductId,
                    productPromotion.DiscountType,
                    productPromotion.DiscountValue
                );
            }
        }

        if (request is CreateOrderPromotionRequest orderPromotionRequest)
        {
            foreach (var orderPromotion in orderPromotionRequest.Levels)
            {
                await _orderPromotionsService.CreateOrderPromotion(
                    promotionResult.Value,
                    orderPromotion.MinValue,
                    orderPromotion.DiscountType,
                    orderPromotion.DiscountValue,
                    orderPromotion.MaxDiscount ?? 0
                );
            }
        }

        return Ok(promotionResult.Value);
    }
}