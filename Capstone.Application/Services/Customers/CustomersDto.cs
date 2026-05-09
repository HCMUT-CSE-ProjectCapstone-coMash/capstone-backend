namespace Capstone.Application.Services.Customers;

public record CustomerDto (
    Guid Id,
    string CustomerName,
    string CustomerPhone,
    string CustomerStatus,
    DateTime CreatedAt,
    double DebitMoney,
    int DebitDays
);

public class NewCustomerStatsDto
{
    public int TodayCount { get; set; }
    public int YesterdayCount { get; set; }
}