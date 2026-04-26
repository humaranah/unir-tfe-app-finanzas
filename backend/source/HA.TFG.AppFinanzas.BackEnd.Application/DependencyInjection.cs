using FluentValidation;
using HA.TFG.AppFinanzas.BackEnd.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace HA.TFG.AppFinanzas.BackEnd.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
