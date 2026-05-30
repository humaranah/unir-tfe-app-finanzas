namespace HA.TFG.AppFinanzas.Core.Movimientos;

public sealed record ComprobanteExtraidoDto
{
    public string? Establecimiento { get; init; }
    public string? Concepto { get; init; }
    public decimal? Importe { get; init; }
    public string? Moneda { get; init; }
    public DateTimeOffset? FechaMovimiento { get; init; }
    public string? TipoMovimiento { get; init; }
    public Guid? IdCuentaCategoria { get; init; }
    public string? CategoriaPropuesta { get; init; }
    public string? Nota { get; init; }
}
