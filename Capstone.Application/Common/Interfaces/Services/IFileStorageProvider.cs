namespace Capstone.Application.Common.Interfaces.Services;

public interface IFileStorageProvider
{
    Task UploadImageAsync(Guid productID, Stream fileStream, string contentType, string extension);
    Task<string> GetImageUrlAsync(string imageKey, int expirationInMinutes = 60);
    Task DeleteImageAsync(string imageKey);
}