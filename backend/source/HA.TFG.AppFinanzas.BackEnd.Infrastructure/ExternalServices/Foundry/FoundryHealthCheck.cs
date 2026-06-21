using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

internal sealed class FoundryHealthCheck(IOptions<FoundryConfig> options) : IHealthCheck
{
    private readonly FoundryConfig _config = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ProjectEndpoint))
        {
            return HealthCheckResult.Degraded("Foundry no configurado (ProjectEndpoint vacío).");
        }

        try
        {
            var projectClient = new AIProjectClient(
                new Uri(_config.ProjectEndpoint),
                new DefaultAzureCredential());

            var agent = projectClient.AsAIAgent(
                model: _config.DeploymentName,
                instructions: "health-check");

            var response = await agent.RunAsync("ping", cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(response?.Text))
            {
                return HealthCheckResult.Unhealthy("Foundry respondió vacío en health check.");
            }

            return HealthCheckResult.Healthy("Conectividad con Foundry OK.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al validar conectividad con Foundry.", ex);
        }
    }
}
