namespace Capstone.Contracts.Products;

public record GetProductsResponse(
    List<ProductResponse> Items,
    int Total
);