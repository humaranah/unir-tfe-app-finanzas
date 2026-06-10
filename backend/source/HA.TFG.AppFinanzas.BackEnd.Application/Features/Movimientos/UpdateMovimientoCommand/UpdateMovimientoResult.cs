namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;

public record UpdateMovimientoResult
{
    public Guid IdMovimiento { get; init; }
    public required string Concepto { get; init; }
    public string? Establecimiento { get; init; }
    public decimal Importe { get; init; }
    public required string Moneda { get; init; }
    public decimal? TipoCambio { get; init; }
    public string Nota { get; init; } = string.Empty;
    public DateTime FechaMovimiento { get; init; }
    public DateTime FechaModificacion { get; init; }
}
