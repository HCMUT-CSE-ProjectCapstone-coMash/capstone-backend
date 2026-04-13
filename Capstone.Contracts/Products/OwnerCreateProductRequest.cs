using Microsoft.AspNetCore.Http;

namespace Capstone.Contracts.Products;

public record OwnerCreateProductRequest(
    string ProductName,
    string Category,
    string Color,
    string? Pattern,
    string SizeType,
    List<ProductQuantity> Quantities,
    string CreatedBy,
    IFormFile? Image,
    double SalePrice,
    double ImportPrice
);