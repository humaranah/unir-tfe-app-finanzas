using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;

public record UpdateMovimientoCommand : IRequest<UpdateMovimientoResult>
{
    public string Email { get; init; } = string.Empty;
    public Guid IdCuenta { get; init; }
    public Guid IdMovimiento { get; init; }
    public Guid IdCuentaCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; init; }
    public required string Concepto { get; init; }
    public string? Establecimiento { get; init; }
    public decimal Importe { get; init; }
    public required string Moneda { get; init; }
    public decimal? TipoCambio { get; init; }
    public string Nota { get; init; } = string.Empty;
    public DateTime FechaMovimiento { get; init; }
}
