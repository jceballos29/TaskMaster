using System.Net;
using System.Text.Json;
using FluentValidation;
using TaskMaster.Domain.Exceptions;

namespace TaskMaster.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pasa la solicitud al siguiente componente en el pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Si cualquier cosa falla en la aplicación, cae aquí
            _logger.LogError(ex, "Ocurrió un error no manejado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // 1. Determinar el código de estado (HTTP Status) usando Pattern Matching
        var statusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.UnprocessableEntity, // 422
            DomainException domainEx => (int)domainEx.HttpStatusCode, // Ej: 400 o 409
            KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
            _ => (int)HttpStatusCode.InternalServerError, // 500 (Fallback)
        };

        // 2. Extraer detalles de los errores (útil para FluentValidation)
        var errors = exception switch
        {
            ValidationException validationEx => validationEx
                .Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
            _ => null,
        };

        // 3. Construir la respuesta estándar
        var response = new
        {
            status = statusCode,
            title = GetTitle(exception),
            detail = exception.Message,
            errors,
        };

        // 4. Escribir la respuesta en formato JSON
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ValidationException => "Validation Error",
            DomainException => "Domain Error",
            KeyNotFoundException => "Not Found",
            UnauthorizedAccessException => "Unauthorized",
            _ => "Internal Server Error",
        };
}
