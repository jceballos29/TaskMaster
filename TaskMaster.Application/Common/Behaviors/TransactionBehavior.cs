using MediatR;
using Microsoft.Extensions.Logging;
using TaskMaster.Application.Common.Interfaces;
using TaskMaster.Domain.Interfaces;

namespace TaskMaster.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger
    )
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // 1. Si es un Query (lectura), saltamos la transacción y continuamos el flujo normal
        if (request is not ICommand<TResponse> && request is not ICommand)
        {
            return await next();
        }

        _logger.LogInformation("Iniciando transacción para {CommandName}", typeof(TRequest).Name);

        try
        {
            // 2. Abrimos transacción
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 3. Ejecutamos el Handler (Lógica de negocio y DbContext.Add/Update)
            var response = await next();

            // 4. Guardamos cambios y confirmamos transacción
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Transacción confirmada para {CommandName}",
                typeof(TRequest).Name
            );

            return response;
        }
        catch (Exception ex)
        {
            // 5. Si explota el Handler, el dominio, o la BD, revertimos todo de forma segura
            _logger.LogError(
                ex,
                "Error ejecutando {CommandName}. Revirtiendo transacción.",
                typeof(TRequest).Name
            );
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
