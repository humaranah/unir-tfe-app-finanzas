namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

/// <summary>
/// Servicio para analizar comprobantes de movimientos mediante OCR/IA.
/// </summary>
public interface IComprobanteAnalysisService
{
    /// <summary>
    /// Analiza un comprobante y extrae datos estructurados (importe, fecha, comercio, etc.).
    /// </summary>
    /// <param name="stream">Contenido del archivo (PDF o imagen).</param>
    /// <param name="contentType">Tipo MIME del archivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado del análisis, o null si el servicio no está disponible.</returns>
    Task<ComprobanteAnalysisResult?> AnalyzeAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken);
}
