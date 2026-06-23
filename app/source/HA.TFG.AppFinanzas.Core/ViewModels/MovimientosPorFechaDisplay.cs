namespace HA.TFG.AppFinanzas.Core.ViewModels;

public sealed class MovimientosPorFechaDisplay(DateOnly fecha, IEnumerable<MovimientoFilaItem> items)
    : List<MovimientoFilaItem>(items)
{
    public DateOnly Fecha { get; } = fecha;
}
