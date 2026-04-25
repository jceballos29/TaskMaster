using Microsoft.Extensions.DependencyInjection;

namespace TaskMaster.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        string? mediatRLicense = null
    )
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Escanea este proyecto y registra automáticamente todos los Handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            if (!string.IsNullOrWhiteSpace(mediatRLicense))
            {
                cfg.LicenseKey = mediatRLicense;
            }
        });
        return services;
    }
}
