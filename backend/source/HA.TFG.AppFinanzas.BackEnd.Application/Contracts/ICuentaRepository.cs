using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface ICuentaRepository
{
    Task<IReadOnlyList<Cuenta>> GetCuentasByUsuarioIdAsync(Guid idUsuario, CancellationToken cancellationToken);
    Task<Cuenta?> GetCuentaByIdAsync(Guid idUsuario, Guid idCuenta, CancellationToken cancellationToken);
    Task<Cuenta> CreateCuentaConCategoriasAsync(Cuenta cuenta, CancellationToken cancellationToken);
    Task<IReadOnlyList<CuentaCategoria>> GetCategoriasByCuentaAsync(Guid idUsuario, Guid idCuenta, CancellationToken cancellationToken);
    Task<CuentaCategoria?> GetCategoriaPorNombreAsync(Guid idCuenta, string nombre, CancellationToken cancellationToken);
}
