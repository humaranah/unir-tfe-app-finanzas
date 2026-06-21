using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.UpdateCuentaCategoriaCommand;

public record UpdateCuentaCategoriaCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
    public Guid IdCuenta { get; init; }
    public Guid IdCuentaCategoria { get; init; }
    public required string Nombre { get; init; }
    public TipoMovimiento TipoMovimiento { get; init; }
}
