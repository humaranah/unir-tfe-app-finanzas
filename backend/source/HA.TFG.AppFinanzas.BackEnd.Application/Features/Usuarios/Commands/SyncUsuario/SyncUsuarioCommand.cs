using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;

public record SyncUsuarioCommand(
    string IdAuth0,
    string Email,
    string Nombre) : IRequest<SyncUsuarioResult>;
