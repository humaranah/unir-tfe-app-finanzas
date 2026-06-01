namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

/// <summary>
/// Servicio genérico que envía un prompt a un LLM y devuelve la respuesta en texto.
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Envía el prompt al LLM y devuelve su respuesta.
    /// </summary>
    /// <param name="prompt">Prompt completo listo para enviar al modelo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <param name="instructions">
    /// Instrucciones de sistema opcionales que definen el rol del modelo para esta llamada.
    /// Si es <c>null</c>, se usan las instrucciones por defecto del servicio.
    /// </param>
    /// <returns>Respuesta del LLM, o null si el servicio no está disponible.</returns>
    Task<string?> EnviarPromptAsync(
        string prompt,
        CancellationToken cancellationToken,
        string? instructions = null);
}
