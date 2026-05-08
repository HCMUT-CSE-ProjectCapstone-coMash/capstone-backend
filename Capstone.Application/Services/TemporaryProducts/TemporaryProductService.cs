using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Application.Services.FileStorageService;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.TemporaryProducts;

public class TemporaryProductsService : ITemporaryProductsService
{
    private readonly IPromptProvider _promptProvider;
    private readonly ITemporaryProductsRepository _temporaryProductRepository;
    private readonly IFileStorageService _fileStorageService;

    public TemporaryProductsService(
        IPromptProvider promptProvider,
        ITemporaryProductsRepository temporaryProductRepository,
        IFileStorageService fileStorageService
    )
    {
        _promptProvider = promptProvider;
        _temporaryProductRepository = temporaryProductRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> CreateTemporaryProduct(string ImageBase64, string ImageKey, string UserId)
    {
        var analyzeResult = await _promptProvider.AnalyzeImageWithClaude(ImageBase64);

        var productName = analyzeResult.Category + " " + analyzeResult.Color + " " + analyzeResult.Pattern;

        var temporaryProduct = new TemporaryProduct
        {
            Id = Guid.NewGuid(),
            ProductName = productName,
            Category = analyzeResult.Category,
            Color = analyzeResult.Color,
            Pattern = analyzeResult.Pattern,
            ImageKey = ImageKey,
            CreatedBy = Guid.Parse(UserId),
        };

        await _temporaryProductRepository.CreateTemporaryProduct(temporaryProduct);

        return Result.Success();
    }

    public async Task<Result<List<TemporaryProductDto>>> GetTemporaryProductsByUserId(string UserId)
    {
        var result = await _temporaryProductRepository.GetTemporaryProductsByUserId(Guid.Parse(UserId));

        var temporaryProductDtos = new List<TemporaryProductDto>();

        foreach (var item in result)
        {
            var imageUrl = "";
            if (!string.IsNullOrEmpty(item.ImageKey))
            {
                var imageResult = await _fileStorageService.GetImageUrlAsync(item.ImageKey);
                imageUrl = imageResult.IsSuccess ? imageResult.Value : "";
            }

            temporaryProductDtos.Add(new TemporaryProductDto
            {
                Id = item.Id,
                ProductName = item.ProductName,
                Category = item.Category,
                Color = item.Color,
                Pattern = item.Pattern,
                ImageUrl = imageUrl
            });
        }

        return Result<List<TemporaryProductDto>>.Success(temporaryProductDtos);
    }

    public async Task<Result> DeleteTemporaryProduct(string TemporaryProductId)
    {
        var temporaryProduct = await _temporaryProductRepository.GetTemporaryProductById(Guid.Parse(TemporaryProductId));

        if (temporaryProduct == null)
        {
            return Result.Failure(new Error("TemporaryProductNotFound", "Temporary product not found."));
        }

        await _fileStorageService.DeleteImageAsync(temporaryProduct.ImageKey);

        await _temporaryProductRepository.DeleteTemporaryProduct(Guid.Parse(TemporaryProductId));

        return Result.Success();
    }
}