using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;

public sealed class SyncUsuarioCommandHandler(
    IUsuarioRepository usuarioRepository,
    IRolRepository rolRepository)
    : IRequestHandler<SyncUsuarioCommand, SyncUsuarioResult>
{
    public async ValueTask<SyncUsuarioResult> Handle(
        SyncUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAuth0Async(command.IdAuth0, cancellationToken);

        if (usuario is not null)
        {
            // Usuario existente: actualiza email y nombre si cambiaron en Auth0
            if (usuario.Email == command.Email && usuario.Nombre == command.Nombre)
                return new SyncUsuarioResult(
                    usuario.Id, usuario.IdAuth0, usuario.Email, usuario.Nombre, EsNuevo: false);

            usuario = usuario with
            {
                Email = command.Email,
                Nombre = command.Nombre,
                FechaModificacion = DateTime.UtcNow
            };

            usuario = await usuarioRepository.ActualizarAsync(usuario, cancellationToken);

            return new SyncUsuarioResult(
                usuario.Id, usuario.IdAuth0, usuario.Email, usuario.Nombre, EsNuevo: false);
        }

        // Usuario nuevo: obtener rol "usuario" y asignárselo
        var rolUsuario = await rolRepository.ObtenerPorNombreAsync(Roles.Usuario, cancellationToken)
            ?? throw new InvalidOperationException(
                $"El rol '{Roles.Usuario}' no existe en la base de datos. Ejecuta el seed de datos.");

        var nuevo = new Usuario
        {
            IdAuth0 = command.IdAuth0,
            Email = command.Email,
            Nombre = command.Nombre,
            FechaCreacion = DateTime.UtcNow,
            Roles = [rolUsuario]
        };

        nuevo = await usuarioRepository.CrearAsync(nuevo, cancellationToken);

        return new SyncUsuarioResult(
            nuevo.Id, nuevo.IdAuth0, nuevo.Email, nuevo.Nombre, EsNuevo: true);
    }
}
