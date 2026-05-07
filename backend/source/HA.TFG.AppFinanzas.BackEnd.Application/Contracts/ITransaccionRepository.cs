using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface ITransaccionRepository
{
    Task<IReadOnlyList<Transaccion>> GetTransaccionesAsync(
        long idCuenta,
        long? idCategoria,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken);
}
