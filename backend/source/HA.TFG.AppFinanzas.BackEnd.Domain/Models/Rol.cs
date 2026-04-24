using HA.TFG.AppFinanzas.BackEnd.Domain.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Rol : ISoftDeleteable
{
    public long Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaEliminacion { get; init; }

    public ICollection<Usuario> Usuarios { get; init; } = [];
}
