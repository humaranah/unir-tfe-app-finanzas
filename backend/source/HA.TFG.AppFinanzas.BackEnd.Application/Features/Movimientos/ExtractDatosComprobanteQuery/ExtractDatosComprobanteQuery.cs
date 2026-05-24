using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ExtractDatosComprobanteQuery;

public record ExtractDatosComprobanteQuery : IRequest<string?>
{
    public required string TextoComprobante { get; init; }
}
