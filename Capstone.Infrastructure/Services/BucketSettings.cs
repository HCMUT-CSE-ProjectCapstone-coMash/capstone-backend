namespace Capstone.Infrastructure.Services;

public class BucketSettings
{
    public string BucketName { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}