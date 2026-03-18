using System.Text;
using Amazon.S3;
using Capstone.Application.Common.Interfaces.Authentication;
using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Application.Common.Interfaces.Services;
using Capstone.Infrastructure.Authentication;
using Capstone.Infrastructure.Persistence;
using Capstone.Infrastructure.Persistence.Repositories;
using Capstone.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Capstone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        services.Configure<BucketSettings>(config.GetSection("BucketSettings"));
        services.Configure<VectorStoreSettings>(config.GetSection("VectorStoreSettings"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IProductQuantitiesRepository, ProductQuantitiesRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IFileStorageProvider, FileStorageProvider>();
        services.AddSingleton<IVectorStoreProvider, VectorStoreProvider>();

        // Image Saving
        services.AddSingleton<IAmazonS3, AmazonS3Client>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<BucketSettings>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = settings.Endpoint,
                ForcePathStyle = true
            };

            return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
        });

        // Vector Store Provider
        services.AddHttpClient<IVectorStoreProvider, VectorStoreProvider>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<VectorStoreSettings>>().Value;
            client.BaseAddress = new Uri(settings.DatabaseURL);
            client.DefaultRequestHeaders.Add("X-API-Key", settings.APIKey);
        });

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = config["JwtSettings:Issuer"],
                        ValidAudience = config["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]!))
                    };
                    
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Cookies["accessToken"];

                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

        return services;
    }
}