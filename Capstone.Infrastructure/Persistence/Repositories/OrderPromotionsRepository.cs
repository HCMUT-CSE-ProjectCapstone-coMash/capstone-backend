using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class OrderPromotionsRepository : IOrderPromotionsRepository
{
    private readonly AppDbContext _context;

    public OrderPromotionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateOrderPromotion(OrderPromotion orderPromotion)
    {
        _context.OrderPromotions.Add(orderPromotion);
        await _context.SaveChangesAsync();
    }
}