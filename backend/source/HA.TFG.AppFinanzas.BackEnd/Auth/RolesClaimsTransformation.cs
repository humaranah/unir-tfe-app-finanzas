using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Auth;

public sealed class RolesClaimsTransformation(AppDbContext context) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var idAuth0 = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(idAuth0))
            return principal;

        var roles = await context.UsuarioIdentidades
            .Where(i => i.IdAuth0 == idAuth0)
            .SelectMany(i => context.Usuarios
                .Where(u => u.Id == i.IdUsuario)
                .SelectMany(u => u.Roles))
            .Select(r => r.Nombre)
            .ToListAsync();

        if (roles.Count == 0)
            return principal;

        var identity = new ClaimsIdentity();
        identity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        principal.AddIdentity(identity);
        return principal;
    }
}
