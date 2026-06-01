using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

/// <summary>
/// Implementación nula del servicio LLM.
/// Se usa cuando el proveedor no está configurado o no está disponible.
/// </summary>
internal sealed class NullLlmService : ILlmService
{
    public Task<string?> EnviarPromptAsync(
        string prompt,
        CancellationToken cancellationToken,
        string? instructions = null)
        => Task.FromResult<string?>(null);
}
