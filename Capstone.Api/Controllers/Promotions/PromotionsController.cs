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

    public PromotionsController(
        IPromotionsService promotionsService,
        IProductPromotionsService productPromotionsService
    )
    {
        _promotionsService = promotionsService;
        _productPromotionsService = productPromotionsService;
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
            request.PromotionId,
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

        return Ok(promotionResult.Value);
    }
}