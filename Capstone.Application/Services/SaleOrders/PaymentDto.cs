namespace Capstone.Application.Services.SaleOrders;

public class PaymentDto
{
    public string Bin { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
}