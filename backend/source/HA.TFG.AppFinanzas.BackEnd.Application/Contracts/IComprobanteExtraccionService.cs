namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

/// <summary>
/// Servicio que envía un prompt a un LLM y devuelve la respuesta en texto.
/// </summary>
public interface IComprobanteExtraccionService
{
    /// <summary>
    /// Envía el prompt al LLM y devuelve su respuesta.
    /// </summary>
    /// <param name="prompt">Prompt completo listo para enviar al modelo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Respuesta del LLM, o null si el servicio no está disponible.</returns>
    Task<string?> EnviarPromptAsync(string prompt, CancellationToken cancellationToken);
}
