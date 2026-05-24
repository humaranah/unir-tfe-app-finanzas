using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.AnalyzeComprobanteQuery;

public record AnalyzeComprobanteQuery : IRequest<ComprobanteAnalysisResult?>
{
    public required Stream ComprobanteStream { get; init; }
    public required string ContentType { get; init; }
}
