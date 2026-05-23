using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface ICuentaRepository
{
    Task<IReadOnlyList<Cuenta>> GetCuentasByUsuarioIdAsync(Guid idUsuario, CancellationToken cancellationToken);
    Task<Cuenta?> GetCuentaByIdAsync(Guid idUsuario, Guid idCuenta, CancellationToken cancellationToken);
    Task<Cuenta> CreateCuentaWithCategoriasAsync(Cuenta cuenta, CancellationToken cancellationToken);
    Task<IReadOnlyList<CuentaCategoria>> GetCategoriasByCuentaAsync(Guid idUsuario, Guid idCuenta, CancellationToken cancellationToken);
    Task<CuentaCategoria?> GetCategoriaByNombreAsync(Guid idCuenta, string nombre, CancellationToken cancellationToken);
    Task<CuentaCategoria?> GetCategoriaByIdAsync(Guid idCuenta, Guid idCuentaCategoria, CancellationToken cancellationToken);
}
