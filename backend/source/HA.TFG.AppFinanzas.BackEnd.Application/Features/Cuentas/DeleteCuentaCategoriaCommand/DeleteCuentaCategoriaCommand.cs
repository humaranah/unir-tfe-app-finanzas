using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.DeleteCuentaCategoriaCommand;

public record DeleteCuentaCategoriaCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
    public Guid IdCuenta { get; init; }
    public Guid IdCuentaCategoria { get; init; }
}
