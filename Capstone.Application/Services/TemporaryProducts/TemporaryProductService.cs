using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Domain.Entities;

namespace Capstone.Application.Services.TemporaryProducts;

public class TemporaryProductsService : ITemporaryProductsService
{
    private readonly IPromptProvider _promptProvider;
    private readonly ITemporaryProductsRepository _temporaryProductRepository;

    public TemporaryProductsService(IPromptProvider promptProvider, ITemporaryProductsRepository temporaryProductRepository)
    {
        _promptProvider = promptProvider;
        _temporaryProductRepository = temporaryProductRepository;
    }

    public async Task<Result> CreateTemporaryProduct(string ImageBase64, string ImageKey)
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
            ImageKey = ImageKey
        };

        await _temporaryProductRepository.CreateTemporaryProduct(temporaryProduct);

        return Result.Success();
    }
}