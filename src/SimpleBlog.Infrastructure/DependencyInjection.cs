using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Azure.Messaging.ServiceBus;
using SimpleBlog.Application.Auth;
using SimpleBlog.Application.Common;
using SimpleBlog.Infrastructure.Caching;
using SimpleBlog.Infrastructure.Identity;
using SimpleBlog.Infrastructure.Messaging;
using SimpleBlog.Infrastructure.Persistence;

namespace SimpleBlog.Infrastructure;

public static class DependencyInjection
{
    // Full registration for the API: persistence, identity, caching + JWT authentication.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistenceAndIdentity(configuration);
        services.AddJwtAuthentication(configuration);
        return services;
    }

    // Registration for non-web hosts (Worker): persistence, identity, caching — no HTTP auth.
    public static IServiceCollection AddInfrastructureWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistenceAndIdentity(configuration);
        return services;
    }

    private static IServiceCollection AddPersistenceAndIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=SimpleBlog;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Cache-aside: use Redis when configured, otherwise an in-memory distributed cache.
        var redis = configuration.GetConnectionString("Redis") ?? configuration["Redis:Configuration"];
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redis;
                options.InstanceName = "SimpleBlog:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, DistributedCacheService>();

        // Optional Azure Service Bus: only wired up when a connection string is provided.
        var serviceBusConn = configuration.GetConnectionString("ServiceBus") ?? configuration["ServiceBus:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(serviceBusConn))
        {
            var sbOptions = new ServiceBusOptions
            {
                ConnectionString = serviceBusConn,
                QueueName = configuration["ServiceBus:QueueName"] ?? "post-events"
            };
            services.AddSingleton(sbOptions);
            services.AddSingleton(_ => new ServiceBusClient(sbOptions.ConnectionString));
            services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
        }

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Key))
            jwt.Key = "dev-super-secret-key-change-me-please-32bytes!!";

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };
            });

        return services;
    }
}
