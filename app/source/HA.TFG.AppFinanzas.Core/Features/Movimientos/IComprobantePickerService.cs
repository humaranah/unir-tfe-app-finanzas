using HA.TFG.AppFinanzas.Core.Features.Movimientos.Models;

namespace HA.TFG.AppFinanzas.Core.Features.Movimientos;

public interface IComprobantePickerService
{
    Task<ComprobanteResult?> SeleccionarArchivoAsync(CancellationToken cancellationToken = default);
    Task<ComprobanteResult?> TomarFotoAsync(CancellationToken cancellationToken = default);
}
