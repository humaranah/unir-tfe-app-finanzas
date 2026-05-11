using HA.TFG.AppFinanzas.BackEnd.Domain.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Cuenta : IAuditable, ISoftDeleteable
{
    public long Id { get; init; }
    public required string Moneda { get; init; }
    public required string Descripcion { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public DateTime? FechaEliminacion { get; init; }

    public ICollection<Usuario> Usuarios { get; init; } = [];
    public ICollection<CuentaCategoria> Categorias { get; init; } = [];
}
