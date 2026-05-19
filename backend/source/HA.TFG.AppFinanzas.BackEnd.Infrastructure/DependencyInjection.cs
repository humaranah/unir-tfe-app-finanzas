using Azure.Storage.Blobs;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddRepositories();
        services.AddAuth0(configuration);
        services.AddComprobanteStorage(configuration);

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
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
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<ICuentaRepository, CuentaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IMovimientoRepository, MovimientoRepository>();
    }

    private static void AddAuth0(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<Auth0Config>()
            .Bind(configuration.GetSection(Auth0Config.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IAuth0UserInfoService, Auth0UserInfoService>((sp, client) =>
        {
            var auth0Config = sp.GetRequiredService<IOptions<Auth0Config>>().Value;
            client.BaseAddress = new Uri(auth0Config.Domain.TrimEnd('/') + "/");
        });
    }

    private static void AddComprobanteStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ComprobanteStorageConfig>()
            .Bind(configuration.GetSection(ComprobanteStorageConfig.SectionName));

        var storageConfig = configuration
            .GetSection(ComprobanteStorageConfig.SectionName)
            .Get<ComprobanteStorageConfig>() ?? new ComprobanteStorageConfig();

        if (storageConfig.Provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(storageConfig.AzureConnectionString))
            {
                services.AddScoped<IComprobanteStorageService>(sp =>
                {
                    sp.GetRequiredService<ILogger<NullComprobanteStorageService>>()
                        .LogError("ComprobanteStorage: Provider es 'Azure' pero AzureConnectionString no está configurada. " +
                                  "El almacenamiento de comprobantes está deshabilitado.");
                    return new NullComprobanteStorageService();
                });
            }
            else
            {
                services.AddSingleton(_ => new BlobServiceClient(storageConfig.AzureConnectionString));
                services.AddScoped<IComprobanteStorageService, AzureBlobComprobanteStorageService>();
            }
        }
        else if (storageConfig.Provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IComprobanteStorageService, LocalComprobanteStorageService>();
        }
        else
        {
            services.AddScoped<IComprobanteStorageService>(sp =>
            {
                sp.GetRequiredService<ILogger<NullComprobanteStorageService>>()
                    .LogError("ComprobanteStorage: Provider '{Provider}' no es válido. " +
                              "El almacenamiento de comprobantes está deshabilitado.",
                              storageConfig.Provider);
                return new NullComprobanteStorageService();
            });
        }
    }
}
