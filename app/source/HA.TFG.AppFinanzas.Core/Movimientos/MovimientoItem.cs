namespace HA.TFG.AppFinanzas.Core.Movimientos;

public sealed class MovimientoItem
{
    public Guid IdMovimiento { get; init; }
    public Guid IdCuenta { get; init; }
    public Guid? IdCategoria { get; init; }
    public string? NombreCategoria { get; init; }
    public string TipoMovimiento { get; init; } = string.Empty;
    public string Concepto { get; init; } = string.Empty;
    public decimal Importe { get; init; }
    public string Moneda { get; init; } = string.Empty;
    public DateOnly FechaMovimiento { get; init; }
}
