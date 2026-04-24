namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Transaccion
{
    public long Id { get; init; }
    public long IdCuenta { get; init; }
    public long IdCategoria { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public decimal Monto { get; init; }
    public string Moneda { get; init; } = string.Empty;
    public string MonedaLocal { get; init; } = string.Empty;
    public decimal TipoCambio { get; init; }
    public string Notas { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; set; }

    public Cuenta? Cuenta { get; set; }
    public CuentaCategoria? Categoria { get; set; }
}