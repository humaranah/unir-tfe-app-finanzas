using FluentValidation;
using HA.TFG.AppFinanzas.BackEnd.Application.Common.Behaviors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HA.TFG.AppFinanzas.BackEnd.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ComprobanteConfig>()
            .Bind(configuration.GetSection(ComprobanteConfig.SectionName));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
