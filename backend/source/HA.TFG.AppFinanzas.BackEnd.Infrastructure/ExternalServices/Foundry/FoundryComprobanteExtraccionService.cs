using Azure.AI.Projects;
using Azure.Identity;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

internal sealed class FoundryComprobanteExtraccionService(
    IOptions<FoundryConfig> options,
    ILogger<FoundryComprobanteExtraccionService> logger)
    : IComprobanteExtraccionService
{
    private readonly FoundryConfig _config = options.Value;

    public async Task<string?> EnviarPromptAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var projectClient = new AIProjectClient(
                new Uri(_config.ProjectEndpoint!),
                new DefaultAzureCredential());

            // AsAIAgent usa la Responses API del proyecto: es una llamada directa al modelo,
            // equivalente a chat completion. No crea ningún recurso en Azure.
            var agent = projectClient.AsAIAgent(
                model: _config.DeploymentName,
                instructions: "Eres un asistente que procesa comprobantes y devuelve únicamente JSON estructurado.");

            var response = await agent.RunAsync(prompt, cancellationToken: cancellationToken);
            return response?.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al llamar a Azure AI Foundry.");
            return null;
        }
    }
}
