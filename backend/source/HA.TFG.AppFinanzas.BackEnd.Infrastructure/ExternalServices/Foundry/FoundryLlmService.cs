using Azure.AI.Projects;
using Azure.Identity;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

internal sealed class FoundryLlmService(
    IOptions<FoundryConfig> options,
    ILogger<FoundryLlmService> logger)
    : ILlmService
{
    private readonly FoundryConfig _config = options.Value;

    public async Task<string?> EnviarPromptAsync(
        string prompt,
        CancellationToken cancellationToken,
        string? instructions = null)
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
                instructions: instructions ?? _config.Instructions);

            var response = await agent.RunAsync(prompt, cancellationToken: cancellationToken);

            if (response?.Usage is { } usage)
            {
                logger.LogInformation(
                    "Uso de tokens — Entrada: {InputTokens}, Salida: {OutputTokens}, Total: {TotalTokens}",
                    usage.InputTokenCount, usage.OutputTokenCount, usage.TotalTokenCount);
            }

            return response?.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al llamar a Azure AI Foundry.");
            return null;
        }
    }
}
