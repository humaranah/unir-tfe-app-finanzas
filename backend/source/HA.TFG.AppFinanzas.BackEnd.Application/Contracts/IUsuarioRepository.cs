using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAuth0Async(string idAuth0, CancellationToken cancellationToken);
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Usuario> CreateAsync(Usuario usuario, UsuarioIdentidad identidad, CancellationToken cancellationToken);
    Task AddIdentidadAsync(long idUsuario, UsuarioIdentidad identidad, CancellationToken cancellationToken);
    Task<Usuario> UpdateAsync(Usuario usuario, CancellationToken cancellationToken);
}
