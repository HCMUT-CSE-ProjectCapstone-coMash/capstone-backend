using Capstone.Application.Services.Authentication;
using Capstone.Application.Services.ComboPromotionsService;
using Capstone.Application.Services.Customers;
using Capstone.Application.Services.FileStorageService;
using Capstone.Application.Services.OrderPromotionsService;
using Capstone.Application.Services.ProductPromotionsService;
using Capstone.Application.Services.ProductQuantitesService;
using Capstone.Application.Services.Products;
using Capstone.Application.Services.ProductsOrders;
using Capstone.Application.Services.ProductsOrdersDetailService;
using Capstone.Application.Services.ProductVectorService;
using Capstone.Application.Services.Promotions;
using Capstone.Application.Services.SaleOrderDetails;
using Capstone.Application.Services.SaleOrders;
using Microsoft.Extensions.DependencyInjection;

namespace Capstone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProductsService, ProductsService>();
        services.AddScoped<IProductQuantitiesService, ProductQuantitiesService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IProductsOrdersService, ProductsOrdersService>();
        services.AddScoped<IProductsOrdersDetailService, ProductsOrdersDetailService>();
        services.AddScoped<ICustomersService, CustomersService>();
        services.AddScoped<ISaleOrdersService, SaleOrdersService>();
        services.AddScoped<ISaleOrderDetailsService, SaleOrderDetailsService>();
        services.AddScoped<IPromotionsService, PromotionsService>();
        services.AddScoped<IProductPromotionsService, ProductPromotionsService>();
        services.AddScoped<IOrderPromotionsService, OrderPromotionsService>();
        services.AddScoped<IComboPromotionsService, ComboPromotionsService>();
        services.AddScoped<IProductVectorService, ProductVectorService>();

        return services;
    }
}