using Microsoft.Extensions.DependencyInjection;
using TaskMaster.Domain.Interfaces;
using TaskMaster.Infrastructure.Services;

namespace TaskMaster.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Le decimos a .NET: "Cuando alguien pida un ISystemInfoService, entrégale un SystemInfoService"
        services.AddTransient<ISystemInfoService, SystemInfoService>();
        return services;
    }
}
