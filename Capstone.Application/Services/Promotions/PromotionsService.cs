using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.ComboPromotionsService;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.OrderPromotionsService;
using Capstone.Application.Services.ProductPromotionsService;
using Capstone.Application.Services.Products;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.Promotions;

public class PromotionsService : IPromotionsService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileStorageService _fileStorageService;

    private readonly IPromotionsRepository _promotionsRepository;

    public PromotionsService(
        IDateTimeProvider dateTimeProvider,
        IFileStorageService fileStorageService,
        IPromotionsRepository promotionsRepository
    )
    {
        _dateTimeProvider = dateTimeProvider;
        _fileStorageService = fileStorageService;
        _promotionsRepository = promotionsRepository;
    }

    private Result<string> GetPromotionPhase(DateOnly startDate, DateOnly endDate)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        string phase;

        if (today < startDate)
        {
            phase = PromotionPhase.Upcoming;
        }
        else if (today > endDate)
        {
            phase = PromotionPhase.Expired;
        }
        else
        {
            phase = PromotionPhase.Ongoing;
        }

        return Result<string>.Success(phase);
    }

    public async Task<Result<string>> CreatePromotionId()
    {
        var prefix = "KM";

        var maxNumber = await _promotionsRepository.GetMaxPromotionId(prefix);
        var newId = $"{prefix}-{maxNumber + 1:D5}";

        return Result<string>.Success(newId);
    }

    public async Task<Result<string>> CreatePromotion(
        string promotionName,
        string promotionType,
        string description,
        string startDate,
        string endDate,
        string createdBy
    )
    {
        var promotionIdResult = await CreatePromotionId();

        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionIdResult.Value,
            PromotionName = promotionName,
            PromotionType = promotionType,
            Description = description,
            StartDate = DateOnly.Parse(startDate),
            EndDate = DateOnly.Parse(endDate),
            PromotionStatus = PromotionStatus.Active.ToString(),
            CreatedBy = Guid.Parse(createdBy),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _promotionsRepository.CreatePromotion(promotion);

        return Result<string>.Success(promotion.Id.ToString());
    }

    public async Task<Result<PaginatedResult<PromotionDto>>> FetchPromotions(
        int currentPage,
        int pageSize,
        string? category = null,
        string? search = null
    )
    {
        var (promotions, total) = await _promotionsRepository.FetchPromotions(currentPage, pageSize, category, search);

        var promotionDtos = promotions.Select(p => new PromotionDto
        {
            Id = p.Id,
            PromotionId = p.PromotionId,
            PromotionName = p.PromotionName,
            PromotionType = p.PromotionType,
            Description = p.Description,
            PromotionStatus = p.PromotionStatus,
            PromotionPhase = GetPromotionPhase(p.StartDate, p.EndDate).Value,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Result<PaginatedResult<PromotionDto>>.Success(
            new PaginatedResult<PromotionDto>(promotionDtos, total));
    }

    public async Task<Result<PromotionDto>> GetPromotionById(string id)
    {
        var promotion = await _promotionsRepository.GetPromotionById(Guid.Parse(id));

        if (promotion == null)
        {
            return Result<PromotionDto>.Failure(new Error("PromotionNotFound", "Promotion not found"));
        }

        return Result<PromotionDto>.Success(new PromotionDto
        {
            Id = promotion.Id,
            PromotionId = promotion.PromotionId,
            PromotionName = promotion.PromotionName,
            PromotionType = promotion.PromotionType,
            Description = promotion.Description,
            PromotionStatus = promotion.PromotionStatus,
            PromotionPhase = GetPromotionPhase(promotion.StartDate, promotion.EndDate).Value,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            CreatedAt = promotion.CreatedAt
        });
    }

    public async Task<Result<string>> UpdatePromotion(
        string promotionId,
        string promotionName,
        string startDate,
        string endDate,
        string description
    )
    {
        var promotion = await _promotionsRepository.GetPromotionById(Guid.Parse(promotionId));

        if (promotion == null)
        {
            return Result<string>.Failure(new Error("PromotionNotFound", "Promotion not found"));
        }

        promotion.PromotionName = promotionName;
        promotion.StartDate = DateOnly.Parse(startDate);
        promotion.EndDate = DateOnly.Parse(endDate);
        promotion.Description = description;

        await _promotionsRepository.UpdatePromotion(promotion);

        return Result<string>.Success(promotion.PromotionName);
    }

    public async Task<Result<List<PromotionDto>>> GetProductPromotionsByProductId(string productId)
    {
        var promotions = await _promotionsRepository.GetProductPromotionsByProductId(Guid.Parse(productId));

        var ongoingPromotions = promotions.Where(p =>
            GetPromotionPhase(p.StartDate, p.EndDate).Value == PromotionPhase.Ongoing
        ).ToList();

        var result = new List<PromotionDto>();

        foreach (var promotion in ongoingPromotions)
        {
            var productPromotions = new List<ProductPromotionDto>();

            foreach (var pp in promotion.ProductPromotions)
            {
                var product = pp.Product;

                var imageUrl = "";
                if (!string.IsNullOrEmpty(product.ImageKey))
                {
                    var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
                    imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
                }

                var productDto = new ProductDto
                (
                    product.Id,
                    product.ProductId,
                    product.ProductName,
                    product.Category,
                    product.Color,
                    product.Pattern,
                    product.SizeType,
                    product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
                    product.CreatedBy,
                    product.CreatedAt,
                    product.Status,
                    imageUrl,
                    product.VectorId,
                    product.SalePrice,
                    product.ImportPrice
                );

                productPromotions.Add(new ProductPromotionDto
                {
                    Id = pp.Id,
                    Product = productDto,
                    DiscountType = pp.DiscountType,
                    DiscountValue = pp.DiscountValue
                });
            }

            result.Add(new PromotionDto
            {
                Id = promotion.Id,
                PromotionId = promotion.PromotionId,
                PromotionName = promotion.PromotionName,
                PromotionType = promotion.PromotionType,
                Description = promotion.Description,
                PromotionStatus = promotion.PromotionStatus,
                PromotionPhase = GetPromotionPhase(promotion.StartDate, promotion.EndDate).Value,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                CreatedAt = promotion.CreatedAt,
                ProductDiscounts = productPromotions
            });
        }

        return Result<List<PromotionDto>>.Success(result);
    }

    public async Task<Result<List<PromotionDto>>> GetComboPromotionsByProductId(string productId)
    {
        var promotions = await _promotionsRepository.GetComboPromotionsByProductId(Guid.Parse(productId));

        var ongoingPromotions = promotions.Where(p =>
            GetPromotionPhase(p.StartDate, p.EndDate).Value == PromotionPhase.Ongoing
        ).ToList();

        var result = new List<PromotionDto>();

        foreach (var promotion in ongoingPromotions)
        {
            var comboPromotions = new List<ComboPromotionDto>();

            foreach (var cp in promotion.ComboPromotions)
            {
                var hasInsufficientStock = cp.ComboPromotionDetails.Any(cpd =>
                    cpd.Product.ProductQuantities.Sum(q => q.Quantities) < cpd.Quantity);

                if (hasInsufficientStock)
                {
                    continue;
                }

                var comboPromotionDetails = new List<ComboPromotionDetailDto>();

                foreach (var cpd in cp.ComboPromotionDetails)
                {
                    var product = cpd.Product;

                    var imageUrl = "";
                    if (!string.IsNullOrEmpty(product.ImageKey))
                    {
                        var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
                        imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
                    }

                    var productDto = new ProductDto
                    (
                        product.Id,
                        product.ProductId,
                        product.ProductName,
                        product.Category,
                        product.Color,
                        product.Pattern,
                        product.SizeType,
                        product.ProductQuantities.Select(q => new ProductQuantityDto(q.Size, q.Quantities)).ToList(),
                        product.CreatedBy,
                        product.CreatedAt,
                        product.Status,
                        imageUrl,
                        product.VectorId,
                        product.SalePrice,
                        product.ImportPrice
                    );

                    comboPromotionDetails.Add(new ComboPromotionDetailDto
                    {
                        Id = cpd.Id,
                        Product = productDto,
                        Quantity = cpd.Quantity
                    });
                }

                comboPromotions.Add(new ComboPromotionDto
                {
                    Id = cp.Id,
                    ComboName = cp.ComboName,
                    ComboPrice = cp.ComboPrice,
                    ComboItems = comboPromotionDetails
                });
            }

            if (comboPromotions.Count == 0)
            {
                continue;
            }

            result.Add(new PromotionDto
            {
                Id = promotion.Id,
                PromotionId = promotion.PromotionId,
                PromotionName = promotion.PromotionName,
                PromotionType = promotion.PromotionType,
                Description = promotion.Description,
                PromotionStatus = promotion.PromotionStatus,
                PromotionPhase = GetPromotionPhase(promotion.StartDate, promotion.EndDate).Value,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                CreatedAt = promotion.CreatedAt,
                Combos = comboPromotions
            });
        }

        return Result<List<PromotionDto>>.Success(result);
    }

    public async Task<Result<List<PromotionDto>>> GetActiveOrderPromotions()
    {
        var promotions = await _promotionsRepository.GetOrderPromotions();

        var ongoingPromotions = promotions.Where(p =>
            GetPromotionPhase(p.StartDate, p.EndDate).Value == PromotionPhase.Ongoing
        ).ToList();

        var result = new List<PromotionDto>();

        foreach (var promotion in ongoingPromotions)
        {
            var promotionLevels = promotion.OrderPromotions.Select(pl => new OrderPromotionDto
            {
                MinValue = pl.MinValue,
                DiscountType = pl.DiscountType,
                DiscountValue = pl.DiscountValue,
                MaxDiscount = pl.MaxDiscount
            }).ToList();

            result.Add(new PromotionDto
            {
                Id = promotion.Id,
                PromotionId = promotion.PromotionId,
                PromotionName = promotion.PromotionName,
                PromotionType = promotion.PromotionType,
                Description = promotion.Description,
                PromotionStatus = promotion.PromotionStatus,
                PromotionPhase = GetPromotionPhase(promotion.StartDate, promotion.EndDate).Value,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                CreatedAt = promotion.CreatedAt,
                Levels = promotionLevels
            });
        }

        return Result<List<PromotionDto>>.Success(result);
    }
}