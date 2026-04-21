using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.Products;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.ComboPromotionsService;

public class ComboPromotionsService : IComboPromotionsService
{
    private readonly IFileStorageService _fileStorageService;

    private readonly IComboPromotionsRepository _comboPromotionsRepository;
    private readonly IComboPromotionDetailsRepository _comboPromotionDetailsRepository;

    public ComboPromotionsService(
        IFileStorageService fileStorageService,
        IComboPromotionsRepository comboPromotionsRepository,
        IComboPromotionDetailsRepository comboPromotionDetailsRepository
    )
    {
        _fileStorageService = fileStorageService;

        _comboPromotionsRepository = comboPromotionsRepository;
        _comboPromotionDetailsRepository = comboPromotionDetailsRepository;
    }

    public async Task<Result<string>> CreateComboPromotion(string promotionId, string comboName, decimal comboPrice)
    {
        var newComboPromotion = new ComboPromotion
        {
            Id = Guid.NewGuid(),
            PromotionId = Guid.Parse(promotionId),
            ComboName = comboName,
            ComboPrice = comboPrice
        };

        await _comboPromotionsRepository.CreateComboPromotion(newComboPromotion);

        return Result<string>.Success(newComboPromotion.Id.ToString());
    }

    public async Task<Result> CreateComboPromotionDetail(string comboPromotionId, string productId, int quantity)
    {
        var newComboPromotionDetail = new ComboPromotionDetail
        {
            Id = Guid.NewGuid(),
            ComboPromotionId = Guid.Parse(comboPromotionId),
            ProductId = Guid.Parse(productId),
            Quantity = quantity
        };

        await _comboPromotionDetailsRepository.CreateComboPromotionDetail(newComboPromotionDetail);

        return Result.Success();
    }

    public async Task<Result<List<ComboPromotionDto>>> GetComboPromotionsByPromotionId(Guid promotionId)
    {
        var comboPromotionList = await _comboPromotionsRepository.GetComboPromotionsByPromotionId(promotionId);

        if (comboPromotionList == null)
        {
            return Result<List<ComboPromotionDto>>.Failure(new Error("PromotionNotFound", "Promotion not found"));
        }

        var comboPromotions = new List<ComboPromotionDto>();

        foreach (var cp in comboPromotionList)
        {
            var comboDetails = new List<ComboPromotionDetailDto>();

            foreach (var cpd in cp.ComboPromotionDetails)
            {
                var product = cpd.Product;

                var imageUrl = "";
                if (!string.IsNullOrEmpty(product.ImageKey))
                {
                    var imageResult = await _fileStorageService.GetImageUrlAsync(product.ImageKey);
                    imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
                }

                var productDto = new ProductDto(
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

                comboDetails.Add(new ComboPromotionDetailDto
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
                ComboItems = comboDetails
            });
        }

        return Result<List<ComboPromotionDto>>.Success(comboPromotions);
    }
}