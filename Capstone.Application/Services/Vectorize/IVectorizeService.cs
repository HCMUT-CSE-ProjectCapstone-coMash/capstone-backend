namespace Capstone.Application.Services.Vectorize;

public interface IVectorizeService
{
    Task<float[]> VectorizeImageAsync(string imageUrl);
}