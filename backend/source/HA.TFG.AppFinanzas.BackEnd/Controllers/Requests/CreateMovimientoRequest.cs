using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record CreateMovimientoRequest
{
    public Guid IdCuentaCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; init; }
    public required string Concepto { get; init; }
    public decimal Importe { get; init; }
    public required string Moneda { get; init; }
    public decimal? TipoCambio { get; init; }
    public string? IdComprobante { get; init; }
    public string Nota { get; init; } = string.Empty;
    public DateTime FechaMovimiento { get; init; }
}
