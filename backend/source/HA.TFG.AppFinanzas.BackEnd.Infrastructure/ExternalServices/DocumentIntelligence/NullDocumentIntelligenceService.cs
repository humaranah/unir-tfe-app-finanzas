using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;

/// <summary>
/// Implementación nula del servicio de análisis de comprobantes.
/// Se usa cuando el proveedor no está configurado o no está disponible.
/// </summary>
internal sealed class NullDocumentIntelligenceService : IComprobanteAnalysisService
{
    public Task<ComprobanteAnalysisResult?> AnalyzeAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
        => Task.FromResult<ComprobanteAnalysisResult?>(null);
}
