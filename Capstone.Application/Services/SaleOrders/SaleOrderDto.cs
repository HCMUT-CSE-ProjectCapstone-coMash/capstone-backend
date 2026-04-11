namespace Capstone.Application.Services.SaleOrders;

public class SaleOrderDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid CreatedBy { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public double DebitMoney { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Discount { get; set; }
}