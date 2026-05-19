using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

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
    Task<Movimiento?> GetMovimientoByIdAsync(Guid idCuenta, Guid idMovimiento, CancellationToken cancellationToken);
}
