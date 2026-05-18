using HA.TFG.AppFinanzas.Core.Models;

namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface IUsuarioService
{
    /// <summary>
    /// Intenta restaurar la sesión a partir del refresh token almacenado.
    /// Devuelve la información del usuario si la sesión fue restaurada exitosamente,
    /// o null si no hay sesión guardada o la restauración falló.
    /// </summary>
    Task<UsuarioInfo?> TryRestoreSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia el flujo de autenticación con Auth0.
    /// Devuelve la información del usuario si el login fue exitoso,
    /// null si el usuario canceló, o lanza una excepción en caso de error.
    /// </summary>
    Task<UsuarioInfo?> LoginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cierra la sesión del usuario y limpia los tokens almacenados.
    /// </summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
