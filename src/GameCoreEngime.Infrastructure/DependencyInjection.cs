using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameCoreEngime.Infrastructure;

public static class DependencyInjection
{
    private static readonly bool IsDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    public static IServiceCollection AddUserIdentityInfra(this IServiceCollection services, IConfiguration configuration)
    {
        // AddDatabase(services, configuration);
        // AddMessageBus(services);
        AddApplicationServices(services);
        // AddJwtAuthentication(services, configuration);
        // AddAuthorizationPolicies(services);
        AddCorsConfiguration(services, configuration);

        return services;
    }

    private static void AddCorsConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        var originsString = configuration.GetValue<string>("Cors:AllowedOrigins");
        var allowedOrigins = originsString?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    }

    // private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    // {
    //     var connectionString = IsDevelopment
    //         ? configuration.GetConnectionString("UserIdentityDatabaseDevelopment")
    //         : configuration.GetConnectionString("UserIdentityDatabaseProduction");

    //     services.AddDbContext<Convert.UserIdentityDbContext>(options =>
    //     {
    //         options.UseNpgsql(connectionString);
    //         options.EnableSensitiveDataLogging();
    //         options.EnableDetailedErrors();
    //     });
    // }


    private static void AddApplicationServices(IServiceCollection services)
    {

    }

    // private static void AddAuthorizationPolicies(IServiceCollection services)
    // {
    //     services.AddAuthorization(options =>
    //     {
    //         options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
    //         options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Administrator"));
    //     });
    // }
}