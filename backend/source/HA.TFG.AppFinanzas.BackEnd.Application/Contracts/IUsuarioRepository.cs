using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAuth0Async(string idAuth0, CancellationToken cancellationToken);
    Task<Usuario> CrearAsync(Usuario usuario, CancellationToken cancellationToken);
    Task<Usuario> ActualizarAsync(Usuario usuario, CancellationToken cancellationToken);
}
