using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

public sealed class EnsureUsuarioCommandHandler(
    IUsuarioRepository usuarioRepository,
    IRolRepository rolRepository)
    : IRequestHandler<EnsureUsuarioCommand, EnsureUsuarioResult>
{
    public async ValueTask<EnsureUsuarioResult> Handle(
        EnsureUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByIdAuth0Async(command.IdAuth0, cancellationToken);

        if (usuario is not null)
            return ToResult(usuario, EsNuevo: false);

        // Usuario nuevo: obtener rol "usuario" y asignárselo
        var rolUsuario = await rolRepository.GetByNombreAsync(Roles.Usuario, cancellationToken)
            ?? throw new InvalidOperationException(
                $"El rol '{Roles.Usuario}' no existe en la base de datos. Ejecuta el seed de datos.");

        var nuevo = new Usuario
        {
            IdAuth0 = command.IdAuth0,
            Email = command.Email,
            Nombre = command.Nombre,
            FotoPerfil = command.FotoPerfil,
            Proveedor = command.Proveedor,
            EmailVerificado = command.EmailVerificado,
            UltimaActualizacion = command.UltimaActualizacion,
            FechaCreacion = DateTime.UtcNow,
            Roles = [rolUsuario]
        };

        nuevo = await usuarioRepository.CreateAsync(nuevo, cancellationToken);
        return ToResult(nuevo, EsNuevo: true);
    }

    private static EnsureUsuarioResult ToResult(Usuario u, bool EsNuevo) =>
        new(u.Id, u.IdAuth0, u.Email, u.Nombre,
            u.FotoPerfil, u.Proveedor, u.EmailVerificado, u.UltimaActualizacion,
            EsNuevo);
}
