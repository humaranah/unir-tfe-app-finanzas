namespace HA.TFG.AppFinanzas.Core.Movimientos;

public interface IMovimientosService
{
    Task<IReadOnlyList<MovimientoItem>> GetMovimientosAsync(
        Guid idCuenta,
        GetMovimientosFilters? filters = null,
        CancellationToken cancellationToken = default);

    Task CreateMovimientoAsync(
        CreateMovimientoDto dto,
        CancellationToken cancellationToken = default);
}
