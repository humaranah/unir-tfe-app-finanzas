namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

/// <summary>
/// Resultado del análisis de un comprobante mediante Document Intelligence.
/// Contiene el texto extraído del documento, listo para enviar a un LLM.
/// </summary>
public sealed record ComprobanteAnalysisResult
{
    /// <summary>Texto completo extraído del comprobante, con saltos de línea.</summary>
    public string Texto { get; init; } = string.Empty;
}
