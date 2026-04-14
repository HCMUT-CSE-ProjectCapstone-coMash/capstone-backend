using Capstone.Application.Common;

namespace Capstone.Application.Services.FileStorageService;

public interface IFileStorageService
{
    Task<Result<string>> UploadImageAsync(string folder, string id, Stream fileStream, string contentType, string extension);
    Task<Result<string>> GetImageUrlAsync(string imageKey, int expirationInMinutes = 60);
    Task DeleteImageAsync(string imageKey);
}