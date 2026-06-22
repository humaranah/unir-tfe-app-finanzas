using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;

internal sealed class DocumentIntelligenceHealthCheck(IOptions<DocumentIntelligenceConfig> options) : IHealthCheck
{
    private readonly DocumentIntelligenceConfig _config = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.Endpoint) || string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            return HealthCheckResult.Degraded("Document Intelligence no configurado (Endpoint o ApiKey vacío).");
        }

        try
        {
            var adminClient = new DocumentIntelligenceAdministrationClient(
                new Uri(_config.Endpoint),
                new AzureKeyCredential(_config.ApiKey));

            _ = await adminClient.GetResourceDetailsAsync(cancellationToken);

            return HealthCheckResult.Healthy("Conectividad con Document Intelligence OK.");
        }
        catch (RequestFailedException ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Error en Document Intelligence (status: {ex.Status}).",
                ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error no controlado al validar Document Intelligence.", ex);
        }
    }
}
