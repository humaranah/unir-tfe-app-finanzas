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

            var (statusCode, problem) = exception switch
            {
                ValidationException ve => (StatusCodes.Status422UnprocessableEntity, new ProblemDetails
                {
                    Title = "Error de validación",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Extensions = { ["errors"] = ve.Errors }
                }),
                _ => (StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Status = StatusCodes.Status500InternalServerError
                })
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem);
        }));

        return app;
    }
}
