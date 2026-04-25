using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskMaster.Application.Common.Behaviors;

namespace TaskMaster.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        string? mediatRLicense = null
    )
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // 1. Escanea y registra todos los validadores
        services.AddValidatorsFromAssembly(assembly);

        // 2. Orquesta MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Asignación de licencia oficial (MediatR v14+)
            if (!string.IsNullOrWhiteSpace(mediatRLicense))
            {
                cfg.LicenseKey = mediatRLicense;
            }

            // Registro estricto del Pipeline Behavior
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        return services;
    }
}
