using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

public record CreateCuentaCommand : IRequest<CreateCuentaResult>
{
    public string Email { get; init; } = string.Empty;
    public required string Moneda { get; init; }
    public string Descripcion { get; init; } = string.Empty;
}