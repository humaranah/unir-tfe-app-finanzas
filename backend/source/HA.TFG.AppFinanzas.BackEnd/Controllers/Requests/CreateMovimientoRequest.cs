using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record CreateMovimientoRequest
{
    public Guid? IdCuentaCategoria { get; init; }
    public required TipoMovimiento TipoMovimiento { get; init; }
    public required string Concepto { get; init; }
    public required decimal Importe { get; init; }
    public required string Moneda { get; init; }
    public decimal? TipoCambio { get; init; }
    public string Nota { get; init; } = string.Empty;
    public required DateTime FechaMovimiento { get; init; }
    public IFormFile? Comprobante { get; init; }
}
