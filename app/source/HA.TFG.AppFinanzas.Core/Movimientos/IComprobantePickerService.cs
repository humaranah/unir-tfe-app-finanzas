namespace HA.TFG.AppFinanzas.Core.Movimientos;

public interface IComprobantePickerService
{
    Task<ComprobanteResult?> SeleccionarArchivoAsync(CancellationToken cancellationToken = default);
    Task<ComprobanteResult?> TomarFotoAsync(CancellationToken cancellationToken = default);
}
