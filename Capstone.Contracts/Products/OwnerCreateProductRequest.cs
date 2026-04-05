using Microsoft.AspNetCore.Http;

namespace Capstone.Contracts.Products;

public record OwnerCreateProductRequest(
    string ProductId,
    string ProductName,
    string Category,
    string Color,
    string? Pattern,
    string SizeType,
    List<ProductQuantity> Quantities,
    string CreatedBy,
    IFormFile? Image,
    decimal SalePrice,
    decimal ImportPrice
);