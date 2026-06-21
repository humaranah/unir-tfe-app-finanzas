using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

internal sealed class FoundryHealthCheck(IOptions<FoundryConfig> options) : IHealthCheck
{
    private readonly FoundryConfig _config = options.Value;

    private static readonly TokenRequestContext TokenContext =
        new(["https://cognitiveservices.azure.com/.default"]);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ProjectEndpoint))
            return HealthCheckResult.Degraded("Foundry no configurado (ProjectEndpoint vacío).");

        try
        {
            var credential = new DefaultAzureCredential();
            await credential.GetTokenAsync(TokenContext, cancellationToken);

            return HealthCheckResult.Healthy("Credenciales Azure AI Foundry OK.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al autenticar con Azure AI Foundry.", ex);
        }
    }
}
