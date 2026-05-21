using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Core.Movimientos;

public interface IMovimientosService
{
    Task<IReadOnlyList<MovimientoItem>> GetMovimientosAsync(
        Guid idCuenta,
        GetMovimientosFilters? filters = null,
        CancellationToken cancellationToken = default);

    Task CreateMovimientoAsync(
        Guid idCuenta,
        string concepto,
        decimal importe,
        string moneda,
        TipoMovimiento tipo,
        DateOnly fecha,
        Guid? idCategoria = null,
        CancellationToken cancellationToken = default);
}

