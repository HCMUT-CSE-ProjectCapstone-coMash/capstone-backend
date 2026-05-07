using Microsoft.AspNetCore.Http;

namespace Capstone.Contracts.Products;

public record CreateTemporaryProductRequest(    
    IFormFile Image,
    string ImageBase64
);