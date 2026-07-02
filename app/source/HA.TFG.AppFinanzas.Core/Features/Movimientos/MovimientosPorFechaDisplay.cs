namespace HA.TFG.AppFinanzas.Core.Features.Movimientos;

public sealed class MovimientosPorFechaDisplay(DateOnly fecha, IEnumerable<MovimientoFilaItem> items)
    : List<MovimientoFilaItem>(items)
{
    public DateOnly Fecha { get; } = fecha;
}
