namespace Capstone.Application.Common;

public record PaginatedResult<T>(List<T> Items, int Total);