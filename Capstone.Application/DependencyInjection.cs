using Capstone.Application.Services.Authentication;
using Capstone.Application.Services.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Capstone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProductsService, ProductsService>();

        return services;
    }
}