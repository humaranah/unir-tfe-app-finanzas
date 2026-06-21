using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface ICuentaCategoriaRepository
{
    Task<IReadOnlyList<CuentaCategoria>> GetCategoriasByCuentaAsync(Guid idUsuario, Guid idCuenta, CancellationToken cancellationToken);
    Task<CuentaCategoria?> GetCategoriaByNombreAsync(Guid idCuenta, string nombre, CancellationToken cancellationToken);
    Task<CuentaCategoria?> GetCategoriaByIdAsync(Guid idCuenta, Guid idCuentaCategoria, CancellationToken cancellationToken);
    Task<CuentaCategoria> CreateCategoriaAsync(CuentaCategoria categoria, CancellationToken cancellationToken);
    Task<CuentaCategoria> UpdateCategoriaAsync(CuentaCategoria categoria, CancellationToken cancellationToken);
    Task DeleteCategoriaAsync(CuentaCategoria categoria, CancellationToken cancellationToken);
}
