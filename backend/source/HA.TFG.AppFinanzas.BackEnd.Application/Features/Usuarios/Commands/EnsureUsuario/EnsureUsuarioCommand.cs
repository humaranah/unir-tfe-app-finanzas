using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

public record EnsureUsuarioCommand(
    string IdAuth0,
    string AccessToken) : IRequest<EnsureUsuarioResult>;
