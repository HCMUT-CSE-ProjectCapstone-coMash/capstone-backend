using Capstone.Application.Services.OrderPromotionsService;
using Capstone.Application.Services.SaleOrderDetails;

namespace Capstone.Application.Services.SaleOrders;

public class SaleOrderDto
{
    public Guid Id { get; set; }
    public string SaleOrderId { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public double DebitMoney { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Discount { get; set; }
    public double TotalPrice { get; set; }
    public double TotalProfit { get; set; }
    public double OriginalTotalPrice { get; set; }
    public string? AppliedOrderPromotionName { get; set; }
    public OrderPromotionDto? AppliedOrderPromotion { get; set; }

    public List<SaleOrderDetailDto> Details { get; set; } = new();
}

// -- Income stats --
public class IncomeGroupDto
{
    public string Key { get; set; } = "";
    public double Total { get; set; }
    public double Profit { get; set; }
}

public class IncomeStatsDto
{
    public string Period { get; set; } = "";
    public double Total { get; set; }
    public double TotalProfit { get; set; }
    public List<IncomeGroupDto> Groups { get; set; } = [];
} 

// -- Customer spending stats --
public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Total { get; set; }
}

public class TopCustomerStatsDto
{
    public List<TopCustomerDto> Customers { get; set; } = [];
    public double WalkInTotal { get; set; }
    public double GrandTotal { get; set; }
}

public class DashboardStatsDto
{
    public double TotalSaleToday { get; set; }
    public double ProfitToday { get; set; }
    public int TotalOrderToday { get; set; }
    public double TotalSaleYesterday { get; set; }
    public double ProfitYesterday { get; set; }
    public int TotalOrderYesterday { get; set; }
}