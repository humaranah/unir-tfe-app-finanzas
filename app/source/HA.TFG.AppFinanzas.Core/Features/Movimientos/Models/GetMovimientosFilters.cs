namespace HA.TFG.AppFinanzas.Core.Features.Movimientos.Models;

public sealed class GetMovimientosFilters
{
    public DateOnly? FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }
    public Guid? IdCategoria { get; init; }

    public string ToQueryString()
    {
        var parametros = new List<string>();

        if (FechaDesde.HasValue)
            parametros.Add($"fechaDesde={FechaDesde.Value:yyyy-MM-dd}");
        if (FechaHasta.HasValue)
            parametros.Add($"fechaHasta={FechaHasta.Value:yyyy-MM-dd}");
        if (IdCategoria.HasValue)
            parametros.Add($"idCategoria={IdCategoria.Value}");

        return parametros.Count > 0
            ? "?" + string.Join('&', parametros)
            : string.Empty;
    }
}
