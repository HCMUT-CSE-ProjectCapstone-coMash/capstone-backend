using Capstone.Domain.Entities;

namespace Capstone.Application.Common.Interfaces.Persistence;

public interface IColorRepository
{
    Task<List<Color>> FetchColors();
}