using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.DeleteMovimientoCommand;

public record DeleteMovimientoCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
    public Guid IdCuenta { get; init; }
    public Guid IdMovimiento { get; init; }
}
