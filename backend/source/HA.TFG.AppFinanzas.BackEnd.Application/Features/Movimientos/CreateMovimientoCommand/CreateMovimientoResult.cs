using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;

public record CreateMovimientoResult
{
    public Guid IdMovimiento { get; init; }
    public Guid IdCuenta { get; init; }
    public Guid IdCuentaCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; init; }
    public string Concepto { get; init; } = string.Empty;
    public decimal Importe { get; init; }
    public string Moneda { get; init; } = string.Empty;
    public decimal? TipoCambio { get; init; }
    public string? IdComprobante { get; init; }
    public string Nota { get; init; } = string.Empty;
    public DateTime FechaMovimiento { get; init; }
}
