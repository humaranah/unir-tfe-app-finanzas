using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.AnalyzeComprobanteQuery;

public class AnalyzeComprobanteQueryHandler(IComprobanteAnalysisService analysisService)
    : IRequestHandler<AnalyzeComprobanteQuery, ComprobanteAnalysisResult?>
{
    public async ValueTask<ComprobanteAnalysisResult?> Handle(
        AnalyzeComprobanteQuery request,
        CancellationToken cancellationToken)
        => await analysisService.AnalyzeAsync(
            request.ComprobanteStream,
            request.ContentType,
            cancellationToken);
}
