using Capstone.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class WarmupService : IHostedService
{
    private readonly IModelPromptProvider _modelPromptProvider;
    private readonly ILogger<WarmupService> _logger;

    public WarmupService(IModelPromptProvider modelPromptProvider, ILogger<WarmupService> logger)
    {
        _modelPromptProvider = modelPromptProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up Gemini connection...");
            await _modelPromptProvider.WarmupAsync();
            _logger.LogInformation("Gemini warmup complete.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Gemini warmup failed (non-fatal): {Message}", ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}