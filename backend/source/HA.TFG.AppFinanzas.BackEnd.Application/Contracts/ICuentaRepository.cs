using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface ICuentaRepository
{
    Task<IReadOnlyList<Cuenta>> GetCuentasByUsuarioIdAsync(long usuarioId, CancellationToken cancellationToken);
    Task<Cuenta> CreateCuentaConCategoriasAsync(long usuarioId, IReadOnlyList<Categoria> categorias, CancellationToken cancellationToken);
}
