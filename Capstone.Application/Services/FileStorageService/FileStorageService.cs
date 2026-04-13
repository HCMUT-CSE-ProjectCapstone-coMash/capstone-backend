using Capstone.Application.Common;
using Capstone.Application.Common.Interfaces.Services;

namespace Capstone.Application.Services.FileStorageService;

public class FileStorageService : IFileStorageService
{
    private readonly IFileStorageProvider _fileStorageProvider;

    public FileStorageService(IFileStorageProvider fileStorageProvider)
    {
        _fileStorageProvider = fileStorageProvider;
    }

    public async Task<Result<string>> UploadImageAsync(string folder, string id, Stream fileStream, string contentType, string extension)
    {
        var key = await _fileStorageProvider.UploadImageAsync(folder, id, fileStream, contentType, extension);

        return Result<string>.Success(key);
    }

    public async Task<Result<string>> GetImageUrlAsync(string imageKey, int expirationInMinutes = 60)
    {
        var url = await _fileStorageProvider.GetImageUrlAsync(imageKey, expirationInMinutes);

        return Result<string>.Success(url);
    }

    public async Task DeleteImageAsync(string imageKey)
    {
        await _fileStorageProvider.DeleteImageAsync(imageKey);
    }
}