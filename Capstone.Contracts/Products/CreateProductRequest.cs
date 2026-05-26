using Microsoft.AspNetCore.Http;

namespace Capstone.Contracts.Products;

public record CreateProductRequest(
    string ProductName,
    string CategoryId,
    string ColorId,
    string? PatternId,
    string SizeType,
    List<ProductQuantity> Quantities,
    string CreatedBy,
    IFormFile? Image
);