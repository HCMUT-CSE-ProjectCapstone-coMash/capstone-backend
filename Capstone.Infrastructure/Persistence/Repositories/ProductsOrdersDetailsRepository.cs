using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ProductsOrdersDetailsRepository : IProductsOrdersDetailsRepository
{
    private readonly AppDbContext _context;

    public ProductsOrdersDetailsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateProductsOrdersDetails(ProductsOrdersDetail productsOrdersDetail)
    {
        _context.ProductsOrdersDetails.Add(productsOrdersDetail);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductsOrdersDetail?> GetProductsOrdersDetailsByOrderIdAndProductId(Guid orderId, Guid productId)
    {
        return await _context.ProductsOrdersDetails.FirstOrDefaultAsync(detail => detail.ProductsOrderId == orderId && detail.ProductId == productId);
    }

    public async Task DeleteProductsOrdersDetails(Guid productsOrdersDetailId)
    {
        var productsOrdersDetail = await _context.ProductsOrdersDetails.FindAsync(productsOrdersDetailId);

        if (productsOrdersDetail != null)
        {
            _context.ProductsOrdersDetails.Remove(productsOrdersDetail);
            await _context.SaveChangesAsync();
        }
    }
}