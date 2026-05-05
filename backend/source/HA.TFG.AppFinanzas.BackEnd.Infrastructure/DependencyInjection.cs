using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        var auth0Domain = configuration["Auth0:Domain"]
            ?? throw new InvalidOperationException("Auth0:Domain no está configurado. Revisa appsettings.json o las variables de entorno.");
        services.AddHttpClient<IAuth0UserInfoService, Auth0UserInfoService>(client =>
        {
            client.BaseAddress = new Uri(auth0Domain.TrimEnd('/') + "/");
        });

        return services;
    }
}
