using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.Products;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.ProductPromotionsService;

public class ProductPromotionsService : IProductPromotionsService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IProductPromotionsRepository _productPromotionsRepository;

    public ProductPromotionsService(
        IFileStorageService fileStorageService,
        IProductPromotionsRepository productPromotionsRepository
    )
    {
        _fileStorageService = fileStorageService;
        _productPromotionsRepository = productPromotionsRepository;
    }

    public async Task<Result> CreateProductPromotion(string promotionId, string productId, string discountType, decimal discountValue)
    {
        var productPromotion = new ProductPromotion
        {
            Id = Guid.NewGuid(),
            PromotionId = Guid.Parse(promotionId),
            ProductId = Guid.Parse(productId),
            DiscountType = discountType,
            DiscountValue = discountValue
        };

        await _productPromotionsRepository.CreateProductPromotion(productPromotion);

        return Result.Success();
    }

    public async Task<Result<List<ProductPromotionDto>>> GetProductPromotionsByPromotionId(Guid promotionId)
    {
        var productPromotionList = await _productPromotionsRepository.GetProductPromotionsByPromotionId(promotionId);

        if (productPromotionList == null)
        {
            return Result<List<ProductPromotionDto>>.Failure(new Error("PromotionNotFound", "Promotion not found"));
        }

        var productPromotions = new List<ProductPromotionDto>();

        foreach (var pp in productPromotionList)
        {
            var product = pp.Product;

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

            var productPromotionDto = new ProductPromotionDto
            {
                Id = pp.Id,
                Product = productDto,
                DiscountType = pp.DiscountType,
                DiscountValue = pp.DiscountValue
            };

            productPromotions.Add(productPromotionDto);
        }

        return Result<List<ProductPromotionDto>>.Success(productPromotions);
    }

    public async Task<Result> DeleteProductPromotionsByPromotionId(Guid promotionId)
    {
        await _productPromotionsRepository.DeleteProductPromotionsByPromotionId(promotionId);

        return Result.Success();
    }
}