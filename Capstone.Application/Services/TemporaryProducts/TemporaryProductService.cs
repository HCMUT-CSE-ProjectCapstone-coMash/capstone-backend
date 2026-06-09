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
    private readonly ICategoryRepository _categoryRepository;
    private readonly IColorRepository _colorRepository;
    private readonly IPatternRepository _patternRepository;

    private readonly IFileStorageService _fileStorageService;

    public TemporaryProductsService(
        IPromptProvider promptProvider,
        ITemporaryProductsRepository temporaryProductRepository,
        ICategoryRepository categoryRepository,
        IColorRepository colorRepository,
        IPatternRepository patternRepository,
        IFileStorageService fileStorageService
    )
    {
        _promptProvider = promptProvider;
        _temporaryProductRepository = temporaryProductRepository;
        _categoryRepository = categoryRepository;
        _colorRepository = colorRepository;
        _patternRepository = patternRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> CreateTemporaryProduct(string ImageBase64, string ImageKey, string UserId)
    {
        var category = await _categoryRepository.FetchCategories();
        var color = await _colorRepository.FetchColors();
        var pattern = await _patternRepository.FetchPatterns();

        var categoryNames = category.Select(c => c.CategoryName).ToList();
        var colorNames = color.Select(c => c.ColorName).ToList();
        var patternNames = pattern.Select(p => p.PatternName).ToList();

        var analyzeResult = await _promptProvider.AnalyzeImageWithClaude(ImageBase64, categoryNames, colorNames, patternNames);

        var matchedCategory = category.FirstOrDefault(c => c.CategoryName.Equals(analyzeResult.Category, StringComparison.OrdinalIgnoreCase));
        var matchedColor = color.FirstOrDefault(c => c.ColorName.Equals(analyzeResult.Color, StringComparison.OrdinalIgnoreCase));
        var matchedPattern = pattern.FirstOrDefault(p => p.PatternName.Equals(analyzeResult.Pattern, StringComparison.OrdinalIgnoreCase));

        if (matchedCategory == null || matchedColor == null || matchedPattern == null)
        {
            return Result.Failure(new Error("AnalysisFailed", "Failed to analyze the image and match category, color, or pattern."));
        }

        var productName = $"{matchedCategory.CategoryName} {matchedColor.ColorName} {matchedPattern.PatternName}";

        var temporaryProduct = new TemporaryProduct
        {
            Id = Guid.NewGuid(),
            ProductName = productName,
            ImageKey = ImageKey,
            CreatedBy = Guid.Parse(UserId),
            CategoryId = matchedCategory.Id,
            ColorId = matchedColor.Id,
            PatternId = matchedPattern.Id
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
                Category = item.Category.CategoryName,
                Color = item.Color.ColorName,
                Pattern = item.Pattern.PatternName,
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