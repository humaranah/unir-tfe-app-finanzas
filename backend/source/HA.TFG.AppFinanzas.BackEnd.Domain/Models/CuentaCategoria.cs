namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record CuentaCategoria
{
    public long Id { get; init; }
    public long IdCuenta { get; init; }
    public long? IdOrigen { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }

    public Cuenta? Cuenta { get; init; }
    public Categoria? Origen { get; init; }
    public ICollection<Transaccion> Transacciones { get; init; } = [];
}
