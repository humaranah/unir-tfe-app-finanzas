using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public sealed record MovimientoFilaItem(MovimientoItem Movimiento, bool IsEven)
{
    public string Concepto => Movimiento.Concepto;
    public TipoMovimiento TipoMovimiento => Movimiento.TipoMovimiento;
    public decimal Importe => Movimiento.Importe;
    public string Moneda => Movimiento.Moneda;
    public DateOnly FechaMovimiento => Movimiento.FechaMovimiento;
}
