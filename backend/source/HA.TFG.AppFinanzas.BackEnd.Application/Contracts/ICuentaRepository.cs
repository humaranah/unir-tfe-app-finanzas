using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface ICuentaRepository
{
    Task<IReadOnlyList<Cuenta>> GetCuentasByUsuarioIdAsync(long idUsuario, CancellationToken cancellationToken);
    Task<Cuenta?> GetCuentaByIdAsync(long idUsuario, long idCuenta, CancellationToken cancellationToken);
    Task<Cuenta> CreateCuentaConCategoriasAsync(Cuenta cuenta, CancellationToken cancellationToken);
}
