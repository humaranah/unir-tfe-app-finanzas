using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

public sealed class EnsureUsuarioCommandHandler(
    IUsuarioRepository usuarioRepository,
    IRolRepository rolRepository,
    IAuth0UserInfoService auth0UserInfoService)
    : IRequestHandler<EnsureUsuarioCommand, EnsureUsuarioResult>
{
    public async ValueTask<EnsureUsuarioResult> Handle(
        EnsureUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var proveedor = ExtractProveedorFromSub(command.IdAuth0);
        var identidad = new UsuarioIdentidad
        {
            IdAuth0 = command.IdAuth0,
            Proveedor = proveedor
        };

        // Caso 1: la identidad (IdAuth0) ya existe → devolver usuario sin llamar a /userinfo
        var usuario = await usuarioRepository.GetByIdAuth0Async(command.IdAuth0, cancellationToken);
        if (usuario is not null)
            return ToResult(usuario, EsNuevo: false);

        // Casos 2 y 3 requieren datos de perfil → obtener desde Auth0 /userinfo
        var userInfo = await auth0UserInfoService.GetUserInfoAsync(command.AccessToken, cancellationToken);

        // Caso 2: el email existe pero con otro proveedor → añadir nueva identidad
        var usuarioPorEmail = await usuarioRepository.GetByEmailAsync(userInfo.Email, cancellationToken);
        if (usuarioPorEmail is not null)
        {
            await usuarioRepository.AddIdentidadAsync(usuarioPorEmail.Id, identidad, cancellationToken);
            return ToResult(usuarioPorEmail, EsNuevo: false);
        }

        // Caso 3: usuario nuevo → crear usuario e identidad
        var rolUsuario = await rolRepository.GetByNombreAsync(Roles.Usuario, cancellationToken)
            ?? throw new InvalidOperationException(
                $"El rol '{Roles.Usuario}' no existe en la base de datos. Ejecuta el seed de datos.");

        var nuevo = new Usuario
        {
            Email = userInfo.Email,
            Nombre = userInfo.Nombre,
            FotoPerfil = userInfo.FotoPerfil,
            EmailVerificado = userInfo.EmailVerificado,
            UltimaActualizacion = userInfo.UltimaActualizacion,
            FechaCreacion = DateTime.UtcNow,
            Roles = [rolUsuario]
        };

        nuevo = await usuarioRepository.CreateAsync(nuevo, identidad, cancellationToken);
        return ToResult(nuevo, EsNuevo: true);
    }

    private static EnsureUsuarioResult ToResult(Usuario u, bool EsNuevo) =>
        new(u.Id, u.Email, u.Nombre,
            u.FotoPerfil, u.EmailVerificado, u.UltimaActualizacion,
            EsNuevo);

    private static string? ExtractProveedorFromSub(string sub)
    {
        var separatorIndex = sub.IndexOf('|');
        return separatorIndex > 0 ? sub[..separatorIndex] : null;
    }
}
