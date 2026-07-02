using HA.TFG.AppFinanzas.Core.Features.Movimientos.Dtos;
using HA.TFG.AppFinanzas.Core.Features.Movimientos.Models;

namespace HA.TFG.AppFinanzas.Core.Features.Movimientos;

public interface IMovimientosService
{
    Task<IReadOnlyList<MovimientoItem>> GetMovimientosAsync(
        Guid idCuenta,
        GetMovimientosFilters? filters = null,
        CancellationToken cancellationToken = default);

    Task<MovimientoDetalleItem> GetMovimientoDetalleAsync(
        Guid idCuenta,
        Guid idMovimiento,
        CancellationToken cancellationToken = default);

    Task<ComprobanteResult?> GetComprobanteAsync(
        Guid idCuenta,
        Guid idMovimiento,
        CancellationToken cancellationToken = default);

    Task CreateMovimientoAsync(
        CreateMovimientoDto dto,
        CancellationToken cancellationToken = default);

    Task UpdateMovimientoAsync(
        UpdateMovimientoDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteMovimientoAsync(
        Guid idCuenta,
        Guid idMovimiento,
        CancellationToken cancellationToken = default);

    Task<ComprobanteExtraidoDto> EscanearComprobanteAsync(
        Guid idCuenta,
        ComprobanteResult comprobante,
        CancellationToken cancellationToken = default);
}
