using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Core.Movimientos;

public sealed record MovimientoDetalleItem
{
    public Guid IdMovimiento { get; init; }
    public Guid IdCuenta { get; init; }
    public Guid IdCuentaCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; init; }
    public string Concepto { get; init; } = string.Empty;
    public string? Establecimiento { get; init; }
    public decimal Importe { get; init; }
    public string Moneda { get; init; } = string.Empty;
    public string Nota { get; init; } = string.Empty;
    public DateTime FechaMovimiento { get; init; }
    public bool TieneComprobante { get; init; }
}
