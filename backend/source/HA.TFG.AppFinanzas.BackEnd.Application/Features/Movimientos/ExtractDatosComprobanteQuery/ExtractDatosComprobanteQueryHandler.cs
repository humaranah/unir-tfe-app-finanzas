using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ExtractDatosComprobanteQuery;

public class ExtractDatosComprobanteQueryHandler(IComprobanteExtraccionService extraccionService)
    : IRequestHandler<ExtractDatosComprobanteQuery, string?>
{
    public async ValueTask<string?> Handle(
        ExtractDatosComprobanteQuery request,
        CancellationToken cancellationToken)
        => await extraccionService.ExtractDatosAsync(request.TextoComprobante, cancellationToken);
}
