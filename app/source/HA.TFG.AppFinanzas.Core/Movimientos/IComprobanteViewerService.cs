namespace HA.TFG.AppFinanzas.Core.Movimientos;

public interface IComprobanteViewerService
{
    Task AbrirComprobanteAsync(ComprobanteResult comprobante, CancellationToken cancellationToken = default);
}
