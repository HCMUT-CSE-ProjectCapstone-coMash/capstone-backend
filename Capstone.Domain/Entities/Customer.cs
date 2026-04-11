namespace Capstone.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public string CustomerStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    public ICollection<SaleOrder> SaleOrders { get; set; } = new List<SaleOrder>();
}