using Capstone.Application.Services.Promotions;
using Capstone.Contracts.Promotions;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Promotions;

[ApiController]
[Route("promotions")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionsService _promotionsService;

    public PromotionsController(IPromotionsService promotionsService)
    {
        _promotionsService = promotionsService;
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

    [HttpPost("create")]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        return Ok(request);
    }
}