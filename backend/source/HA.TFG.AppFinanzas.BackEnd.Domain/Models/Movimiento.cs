using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Movimiento : IAuditable, ISoftDeleteable
{
    public Guid IdMovimiento { get; init; }
    public Guid IdCuenta { get; init; }
    public Guid IdCuentaCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public string Concepto { get; init; } = string.Empty;
    public decimal Importe { get; init; }
    public string Moneda { get; init; } = string.Empty;
    public decimal? TipoCambio { get; init; }
    public string? IdComprobante { get; init; }
    public string Nota { get; init; } = string.Empty;
    public DateTime FechaMovimiento { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public DateTime? FechaEliminacion { get; init; }

    public Cuenta? Cuenta { get; init; }
    public CuentaCategoria? Categoria { get; init; }
}