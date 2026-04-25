using FluentValidation;
using MediatR;
using TaskMaster.Application.Common.Models;

namespace TaskMaster.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            // Ejecuta todos los validadores en paralelo
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            // Agrupa todos los errores encontrados
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                // Si hay errores, lanzamos una excepción de validación que atraparemos globalmente después
                throw new ValidationException(failures);
            }
        }

        // Si todo está correcto, dejamos que el flujo continúe hacia el Handler
        return await next(cancellationToken);
    }
}
