namespace Capstone.Application.Common.Interfaces.Services;

public interface IVectorStoreProvider
{
    Task<string> InsertImageAsync(string imageUrl, object metadata);
}