using Microsoft.AspNetCore.Http;

namespace Capstone.Contracts.Products;

public record FetchSimilarProductsRequest(
    IFormFile Image
);