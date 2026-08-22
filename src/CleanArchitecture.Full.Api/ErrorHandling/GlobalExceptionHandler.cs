// src/CleanArchitecture.Full.Api/ErrorHandling/GlobalExceptionHandler.cs
using CleanArchitecture.Full.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Full.Api.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is FluentValidation.ValidationException validationException)
        {
            _logger.LogWarning(
                "Validación fallida en {Method} {Path}: {Errores}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                validationException.Errors.Select(e => e.ErrorMessage));

            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var validationProblem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Uno o más campos no son válidos",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = validationProblem.Status!.Value;
            await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken: cancellationToken);
            return true;
        }

        var (statusCode, title) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Error no controlado procesando {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Error controlado ({StatusCode}) procesando {Method} {Path}: {Mensaje}",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado. Intente nuevamente más tarde."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ConflictException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual del recurso"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual del recurso"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
        _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
    };
}
