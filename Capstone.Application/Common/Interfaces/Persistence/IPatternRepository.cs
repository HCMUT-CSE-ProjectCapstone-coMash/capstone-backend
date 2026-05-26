using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IPatternRepository
{
    Task<List<Pattern>> FetchPatterns();
}