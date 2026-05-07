using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:SqlServer no está configurada o está vacía. " +
                "Revisa appsettings.json o las variables de entorno del entorno actual.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<ICuentaRepository, CuentaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ITransaccionRepository, TransaccionRepository>();

        services.AddOptions<Auth0Config>()
            .Bind(configuration.GetSection(Auth0Config.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IAuth0UserInfoService, Auth0UserInfoService>((sp, client) =>
        {
            var auth0Config = sp.GetRequiredService<IOptions<Auth0Config>>().Value;
            client.BaseAddress = new Uri(auth0Config.Domain.TrimEnd('/') + "/");
        });

        return services;
    }
}
