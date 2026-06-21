using Azure;
using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Middleware;

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder => builder.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var logger = context.RequestServices.GetRequiredService<ILogger<IExceptionHandlerFeature>>();
            var correlationId = context.TraceIdentifier;

            var (statusCode, problem) = exception switch
            {
                ValidationException ve => HandleValidation(ve, logger),
                ConflictException ce => HandleConflict(ce, logger),
                NotFoundException nfe => HandleNotFound(nfe, logger),
                FileNotFoundException fnfe => HandleFileNotFound(fnfe, logger),
                RequestFailedException rfe => HandleRequestFailed(rfe, logger),
                ExternalServiceException ese => HandleExternalService(ese, logger),
                _ => HandleUnexpected(exception, logger)
            };

            problem.Extensions["correlationId"] = correlationId;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem, problem.GetType());
        }));

        return app;
    }

    private static (int, ProblemDetails) HandleValidation(ValidationException ex, ILogger logger)
    {
        logger.LogWarning(ex, "Error de validación: {Message}", ex.Message);

        var problem = new ValidationProblemDetails(
            ex.Errors.ToDictionary(e => e.Key, e => e.Value))
        {
            Title = "Error de validación",
            Status = StatusCodes.Status400BadRequest
        };

        return (StatusCodes.Status400BadRequest, problem);
    }

    private static (int, ProblemDetails) HandleConflict(ConflictException ex, ILogger logger)
    {
        logger.LogWarning(ex, "Conflicto: {EntityName} con valor '{SafeKey}'", ex.EntityName, ex.SafeKey);

        var problem = new ProblemDetails
        {
            Title = "Conflicto",
            Detail = ex.Message,
            Status = StatusCodes.Status409Conflict
        };

        return (StatusCodes.Status409Conflict, problem);
    }

    private static (int, ProblemDetails) HandleNotFound(NotFoundException ex, ILogger logger)
    {
        if (ex.SafeKey != null)
            logger.LogWarning(ex, "Entidad no encontrada: {EntityName} con clave '{SafeKey}'", ex.EntityName, ex.SafeKey);
        else
            logger.LogWarning(ex, "Entidad no encontrada: {EntityName}", ex.EntityName);

        var problem = new ProblemDetails
        {
            Title = "Recurso no encontrado",
            Detail = ex.Message,
            Status = StatusCodes.Status404NotFound
        };

        return (StatusCodes.Status404NotFound, problem);
    }

    private static (int, ProblemDetails) HandleFileNotFound(FileNotFoundException ex, ILogger logger)
    {
        logger.LogWarning(ex, "Fichero no encontrado: {FileName}", ex.FileName);

        var problem = new ProblemDetails
        {
            Title = "Recurso no encontrado",
            Detail = ex.Message,
            Status = StatusCodes.Status404NotFound
        };

        return (StatusCodes.Status404NotFound, problem);
    }

    private static (int, ProblemDetails) HandleRequestFailed(RequestFailedException ex, ILogger logger)
    {
        if (ex.Status == StatusCodes.Status404NotFound)
        {
            logger.LogWarning(ex, "Comprobante no encontrado en Azure Blob: {ErrorCode}", ex.ErrorCode);

            var notFoundProblem = new ProblemDetails
            {
                Title = "Recurso no encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            };

            return (StatusCodes.Status404NotFound, notFoundProblem);
        }

        logger.LogError(ex, "Error en Azure Blob Storage [{Status}]: {ErrorCode}", ex.Status, ex.ErrorCode);

        var problem = new ProblemDetails
        {
            Title = "Error en servicio externo",
            Detail = ex.Message,
            Status = StatusCodes.Status502BadGateway
        };

        return (StatusCodes.Status502BadGateway, problem);
    }

    private static (int, ProblemDetails) HandleExternalService(ExternalServiceException ex, ILogger logger)
    {
        logger.LogError(ex, "Error en servicio externo '{ServiceName}': {Message}", ex.ServiceName, ex.Message);

        var problem = new ProblemDetails
        {
            Title = "Error en servicio externo",
            Detail = ex.Message,
            Status = StatusCodes.Status502BadGateway
        };

        return (StatusCodes.Status502BadGateway, problem);
    }

    private static (int, ProblemDetails) HandleUnexpected(Exception? ex, ILogger logger)
    {
        logger.LogError(ex, "Error no controlado: {Message}", ex?.Message);

        var problem = new ProblemDetails
        {
            Title = "Error interno del servidor",
            Status = StatusCodes.Status500InternalServerError
        };

        return (StatusCodes.Status500InternalServerError, problem);
    }
}
