using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Extensions;

internal static class AzureStartupDiagnosticsExtensions
{
    internal static void LogAzureServicesConfig(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        // SQL Server
        var connString = app.Configuration.GetConnectionString("SqlServer") ?? string.Empty;
        var dbHost = ExtractHostConnection(connString);
        logger.LogInformation("[Config] SQL Server -> {Host}", dbHost);

        // Document Intelligence
        var diConfig = app.Services.GetRequiredService<IOptions<DocumentIntelligenceConfig>>().Value;
        if (diConfig.Provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            logger.LogInformation("[Config] Document Intelligence -> Azure | Endpoint: {Endpoint} | Modelo: {ModelId}",
                diConfig.Endpoint ?? "(no configurado)", diConfig.ModelId);
        else
            logger.LogWarning("[Config] Document Intelligence -> Proveedor '{Provider}' (servicio deshabilitado)", diConfig.Provider);

        // Azure Blob Storage
        var storageConfig = app.Services.GetRequiredService<IOptions<ComprobanteStorageConfig>>().Value;
        if (storageConfig.Provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            logger.LogInformation("[Config] Blob Storage -> Azure");
        else if (storageConfig.Provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
            logger.LogInformation("[Config] Blob Storage -> Local | Ruta: {Path}",
                storageConfig.LocalBasePath ?? "(no configurada)");
        else
            logger.LogWarning("[Config] Blob Storage -> Proveedor '{Provider}' (servicio deshabilitado)", storageConfig.Provider);

        // Azure AI Foundry
        var foundryConfig = app.Services.GetRequiredService<IOptions<FoundryConfig>>().Value;
        if (foundryConfig.Provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            logger.LogInformation("[Config] Foundry AI -> Azure | Endpoint: {Endpoint} | Modelo: {Model}",
                foundryConfig.ProjectEndpoint ?? "(no configurado)", foundryConfig.DeploymentName);
        else
            logger.LogWarning("[Config] Foundry AI -> Proveedor '{Provider}' (servicio deshabilitado)", foundryConfig.Provider);
    }

    private static string ExtractHostConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return "(no configurado)";
        foreach (var part in connectionString.Split(';'))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 &&
                (kv[0].Trim().Equals("Server", StringComparison.OrdinalIgnoreCase) ||
                 kv[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase)))
                return kv[1].Trim();
        }
        return "(no identificado)";
    }
}
