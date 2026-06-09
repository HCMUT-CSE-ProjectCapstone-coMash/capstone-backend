namespace Capstone.Application.Common.Interfaces.Services;

public interface IVectorizeProvider
{
    Task<float[]> VectorizeImageAsync(string imageUrl);
    Task<float[]> VectorizeImageBase64Async(string imageBase64);
}