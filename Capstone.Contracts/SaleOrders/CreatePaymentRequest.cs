namespace Capstone.Contracts.SaleOrders;

public class CreatePaymentRequest
{
    public int OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}