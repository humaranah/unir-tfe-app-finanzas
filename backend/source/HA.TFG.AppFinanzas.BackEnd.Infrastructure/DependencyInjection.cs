using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
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

        return services;
    }
}
