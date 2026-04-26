using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;
using Riok.Mapperly.Abstractions;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Mappers;

public record Auth0UserClaims(string IdAuth0, string Email, string Nombre);

[Mapper]
public static partial class UsuarioMapper
{
    public static partial SyncUsuarioCommand ToSyncUsuarioCommand(this Auth0UserClaims claims);
}

public static class ClaimsPrincipalMapper
{
    public static SyncUsuarioCommand? ToSyncUsuarioCommand(this ClaimsPrincipal user)
    {
        var claims = user.ToAuth0UserClaims();
        return claims is null ? null : UsuarioMapper.ToSyncUsuarioCommand(claims);
    }

    private static Auth0UserClaims? ToAuth0UserClaims(this ClaimsPrincipal user)
    {
        var idAuth0 = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst("sub")?.Value;

        var email = user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst("email")?.Value;

        var nombre = user.FindFirst("name")?.Value
                     ?? user.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(idAuth0) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(nombre))
            return null;

        return new Auth0UserClaims(idAuth0, email, nombre);
    }
}
