namespace HA.TFG.AppFinanzas.Core.Movimientos;

public sealed class MovimientosPorFecha(DateOnly fecha, IEnumerable<MovimientoItem> items)
    : List<MovimientoItem>(items)
{
    public DateOnly Fecha { get; } = fecha;
}
