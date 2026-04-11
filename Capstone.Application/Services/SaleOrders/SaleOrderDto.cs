using Capstone.Application.Services.SaleOrderDetails;

namespace Capstone.Application.Services.SaleOrders;

public class SaleOrderDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public double DebitMoney { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Discount { get; set; }
    public double TotalPrice { get; set; }
    public List<SaleOrderDetailDto> Details { get; set; } = new();
}