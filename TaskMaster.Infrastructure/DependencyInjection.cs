using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskMaster.Domain.Interfaces;
using TaskMaster.Infrastructure.Persistence;
using TaskMaster.Infrastructure.Services;

namespace TaskMaster.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("DatabaseConnection"),
                    npgsql =>
                    {
                        npgsql.MigrationsHistoryTable("__ef_migrations_history");
                        npgsql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null
                        );
                    }
                )
                .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddTransient<ISystemInfoService, SystemInfoService>();
        return services;
    }
}
