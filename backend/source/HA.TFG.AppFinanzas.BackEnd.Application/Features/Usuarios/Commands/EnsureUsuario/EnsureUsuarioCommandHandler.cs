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
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<EnsureUsuarioResult> Handle(
        EnsureUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var proveedor = ExtractProveedorFromSub(command.IdAuth0);

        var usuario = await _usuarioRepository.GetByIdAuth0Async(command.IdAuth0, cancellationToken);

        if (usuario is not null)
            return ToResult(usuario, EsNuevo: false);

        // Usuario nuevo: obtener rol "usuario" y asignárselo
        var rolUsuario = await _rolRepository.GetByNombreAsync(Roles.Usuario, cancellationToken)
            ?? throw new InvalidOperationException(
                $"El rol '{Roles.Usuario}' no existe en la base de datos. Ejecuta el seed de datos.");

        var nuevo = new Usuario
        {
            IdAuth0 = command.IdAuth0,
            Email = command.Email,
            Nombre = command.Nombre,
            FotoPerfil = command.FotoPerfil,
            Proveedor = proveedor,
            EmailVerificado = command.EmailVerificado,
            UltimaActualizacion = command.UltimaActualizacion,
            FechaCreacion = DateTime.UtcNow,
            Roles = [rolUsuario]
        };

        nuevo = await _usuarioRepository.CreateAsync(nuevo, cancellationToken);
        return ToResult(nuevo, EsNuevo: true);
    }

    private static EnsureUsuarioResult ToResult(Usuario u, bool EsNuevo) =>
        new(u.Id, u.IdAuth0, u.Email, u.Nombre,
            u.FotoPerfil, u.Proveedor, u.EmailVerificado, u.UltimaActualizacion,
            EsNuevo);

    private static string? ExtractProveedorFromSub(string sub)
    {
        var separatorIndex = sub.IndexOf('|');
        if (separatorIndex <= 0)
            return null;

        return sub[..separatorIndex];
    }
}
