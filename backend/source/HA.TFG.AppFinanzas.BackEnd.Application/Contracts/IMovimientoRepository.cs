using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface IMovimientoRepository
{
    Task<IReadOnlyList<Movimiento>> GetMovimientosAsync(
        Guid idCuenta,
        Guid? idCategoria,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken);

    Task<Movimiento> AddMovimientoAsync(Movimiento movimiento, CancellationToken cancellationToken);
    Task<Movimiento> UpdateMovimientoAsync(Movimiento movimiento, CancellationToken cancellationToken);
    Task<Movimiento> DeleteMovimientoAsync(Movimiento movimiento, CancellationToken cancellationToken);
    Task<Movimiento?> GetMovimientoByIdAsync(Guid idCuenta, Guid idMovimiento, CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene el total de gasto agregado por categoría y mes para una cuenta,
    /// considerando únicamente los movimientos de tipo <see cref="TipoMovimiento.Gasto"/>.
    /// </summary>
    /// <param name="idCuenta">Cuenta sobre la que se agregan los gastos.</param>
    /// <param name="fechaDesde">Fecha inicial del rango (inclusive).</param>
    /// <param name="fechaHasta">Fecha final del rango (inclusive).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<IReadOnlyList<ResumenGastoCategoria>> GetResumenGastosPorCategoriaAsync(
        Guid idCuenta,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        CancellationToken cancellationToken);
}
