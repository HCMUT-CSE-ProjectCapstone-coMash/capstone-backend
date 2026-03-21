using Capstone.Application.Services.Authentication;
using Capstone.Application.Services.Products;
using Capstone.Application.Services.ProductsOrders;
using Microsoft.Extensions.DependencyInjection;

namespace Capstone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProductsService, ProductsService>();
        services.AddScoped<IProductsOrdersService, ProductsOrdersService>();

        return services;
    }
}