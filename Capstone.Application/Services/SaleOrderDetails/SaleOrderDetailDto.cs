namespace Capstone.Application.Services.SaleOrderDetails;

public class SaleOrderDetailDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SelectedSize { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double Discount { get; set; }
    public double SubTotal { get; set; }
}