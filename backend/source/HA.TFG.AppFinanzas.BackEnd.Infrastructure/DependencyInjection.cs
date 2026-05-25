using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;
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
        services.AddDocumentIntelligence(configuration);
        services.AddFoundry(configuration);

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

        switch (storageConfig.Provider)
        {
            case ComprobanteStorageProvider.Azure when !string.IsNullOrWhiteSpace(storageConfig.AzureConnectionString):
                services.AddSingleton(_ => new BlobServiceClient(storageConfig.AzureConnectionString));
                services.AddScoped<IComprobanteStorageService, AzureBlobComprobanteStorageService>();
                break;

            case ComprobanteStorageProvider.Azure:
                services.AddLogging();
                using (var sp = services.BuildServiceProvider())
                    sp.GetRequiredService<ILogger<NullComprobanteStorageService>>()
                        .LogError("ComprobanteStorage: Provider es '{Provider}' pero AzureConnectionString no está configurada. " +
                                  "El almacenamiento de comprobantes está deshabilitado.", storageConfig.Provider);
                services.AddScoped<IComprobanteStorageService, NullComprobanteStorageService>();
                break;

            case ComprobanteStorageProvider.Local:
                services.AddScoped<IComprobanteStorageService, LocalComprobanteStorageService>();
                break;

            default:
                services.AddScoped<IComprobanteStorageService, NullComprobanteStorageService>();
                break;
        }
    }

    private static void AddDocumentIntelligence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DocumentIntelligenceConfig>()
            .Bind(configuration.GetSection(DocumentIntelligenceConfig.SectionName));

        var diConfig = configuration
            .GetSection(DocumentIntelligenceConfig.SectionName)
            .Get<DocumentIntelligenceConfig>() ?? new DocumentIntelligenceConfig();

        if (!string.IsNullOrWhiteSpace(diConfig.Endpoint) && !string.IsNullOrWhiteSpace(diConfig.ApiKey))
        {
            services.AddSingleton(_ =>
                new DocumentAnalysisClient(
                    new Uri(diConfig.Endpoint),
                    new Azure.AzureKeyCredential(diConfig.ApiKey)));

            services.AddScoped<IComprobanteAnalysisService, AzureDocumentIntelligenceService>();
        }
        else
        {
            services.AddScoped<IComprobanteAnalysisService, NullDocumentIntelligenceService>();
        }
    }

    private static void AddFoundry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FoundryConfig>()
            .Bind(configuration.GetSection(FoundryConfig.SectionName));

        var foundryConfig = configuration
            .GetSection(FoundryConfig.SectionName)
            .Get<FoundryConfig>() ?? new FoundryConfig();

        if (!string.IsNullOrWhiteSpace(foundryConfig.ProjectEndpoint))
        {
            services.AddScoped<IComprobanteExtraccionService, FoundryComprobanteExtraccionService>();
        }
        else
        {
            services.AddScoped<IComprobanteExtraccionService, NullComprobanteExtraccionService>();
        }
    }
}
