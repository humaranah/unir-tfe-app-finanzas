using Azure.AI.Projects;
using Azure.Identity;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

internal sealed class FoundryComprobanteExtraccionService(
    IOptions<FoundryConfig> options,
    ILogger<FoundryComprobanteExtraccionService> logger)
    : IComprobanteExtraccionService
{
    private readonly FoundryConfig _config = options.Value;

    public async Task<string?> ExtractDatosAsync(string textoComprobante, CancellationToken cancellationToken)
    {
        try
        {
            var projectClient = new AIProjectClient(
                new Uri(_config.ProjectEndpoint!),
                new DefaultAzureCredential());

            AIAgent agent = await projectClient.CreateAIAgentAsync(
                name: $"comprobante-extractor-{Guid.NewGuid():N}",
                model: _config.DeploymentName,
                instructions: _config.SystemPrompt,
                cancellationToken: cancellationToken);

            try
            {
                var response = await agent.RunAsync(
                    textoComprobante,
                    cancellationToken: cancellationToken);

                return response?.ToString();
            }
            finally
            {
                await projectClient.Agents.DeleteAgentAsync(agent.Name, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al llamar a Azure AI Foundry para extracción de datos del comprobante.");
            return null;
        }
    }
}
