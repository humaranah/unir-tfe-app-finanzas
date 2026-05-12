using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientosQuery;

public record GetMovimientosResultItem
{
    public Guid IdMovimiento { get; init; }
    public Guid IdCuenta { get; init; }
    public Guid? IdCategoria { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
    public TipoMovimiento TipoMovimiento { get; init; }
    public string Concepto { get; init; } = string.Empty;
    public decimal Importe { get; init; }
    public string Moneda { get; init; } = string.Empty;
    public DateOnly FechaMovimiento { get; init; }
}
