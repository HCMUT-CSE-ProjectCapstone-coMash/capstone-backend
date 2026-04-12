namespace Capstone.Domain.Entities;

public class SaleOrder
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid CreatedBy { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public double DebitMoney { get; set; }
    public DateTime CreatedAt { get; set; }
    public double TotalPrice { get; set; }

    public Customer? Customer { get; set; }
    public User User { get; set; } = null!;
    public ICollection<SaleOrderDetail> SaleOrderDetails { get; set; } = new List<SaleOrderDetail>();
}