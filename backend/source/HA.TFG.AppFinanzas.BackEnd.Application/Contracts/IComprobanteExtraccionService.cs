namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

/// <summary>
/// Servicio que envía el texto de un comprobante a un LLM y devuelve la respuesta.
/// </summary>
public interface IComprobanteExtraccionService
{
    /// <summary>
    /// Envía el texto extraído de un comprobante al LLM y devuelve su respuesta.
    /// </summary>
    /// <param name="textoComprobante">Texto extraído del comprobante mediante OCR.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Respuesta del LLM, o null si el servicio no está disponible.</returns>
    Task<string?> ExtractDatosAsync(string textoComprobante, CancellationToken cancellationToken);
}
