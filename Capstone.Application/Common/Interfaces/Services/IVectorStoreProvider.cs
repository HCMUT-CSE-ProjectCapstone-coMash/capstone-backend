namespace Capstone.Application.Common.Interfaces.Services;

public interface IVectorStoreProvider
{
    Task<string> InsertImageAsync(Stream imageStream, object metadata);
}