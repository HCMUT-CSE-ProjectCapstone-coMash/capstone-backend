using Capstone.Application.Common.Interfaces.Services;

namespace Capstone.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}