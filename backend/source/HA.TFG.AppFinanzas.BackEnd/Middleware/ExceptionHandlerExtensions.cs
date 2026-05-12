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
                NotFoundException nfe => HandleNotFound(nfe, logger),
                ExternalServiceException ese => HandleExternalService(ese, logger),
                _ => HandleUnexpected(exception, logger)
            };

            problem.Extensions["correlationId"] = correlationId;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem, problem.GetType());
        }));

        return app;
    }

    private static (int, ProblemDetails) HandleValidation(ValidationException ve, ILogger logger)
    {
        logger.LogWarning(ve, "Error de validación: {Message}", ve.Message);

        var problem = new ValidationProblemDetails(
            ve.Errors.ToDictionary(e => e.Key, e => e.Value))
        {
            Title = "Error de validación",
            Status = StatusCodes.Status400BadRequest
        };

        return (StatusCodes.Status400BadRequest, problem);
    }

    private static (int, ProblemDetails) HandleNotFound(NotFoundException nfe, ILogger logger)
    {
        logger.LogWarning(nfe, "Entidad no encontrada: {EntityName} con clave '{Key}'", nfe.EntityName, nfe.Key);

        var problem = new ProblemDetails
        {
            Title = "Recurso no encontrado",
            Detail = nfe.Message,
            Status = StatusCodes.Status404NotFound
        };

        return (StatusCodes.Status404NotFound, problem);
    }

    private static (int, ProblemDetails) HandleExternalService(ExternalServiceException ese, ILogger logger)
    {
        logger.LogError(ese, "Error en servicio externo '{ServiceName}': {Message}", ese.ServiceName, ese.Message);

        var problem = new ProblemDetails
        {
            Title = "Error en servicio externo",
            Detail = ese.Message,
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
