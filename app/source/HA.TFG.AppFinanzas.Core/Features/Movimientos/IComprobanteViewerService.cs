using HA.TFG.AppFinanzas.Core.Features.Movimientos.Models;

namespace HA.TFG.AppFinanzas.Core.Features.Movimientos;

public interface IComprobanteViewerService
{
    Task AbrirComprobanteAsync(ComprobanteResult comprobante, CancellationToken cancellationToken = default);
}
