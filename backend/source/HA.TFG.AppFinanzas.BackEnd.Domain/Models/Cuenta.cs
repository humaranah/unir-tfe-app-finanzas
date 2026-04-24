using HA.TFG.AppFinanzas.BackEnd.Domain.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Cuenta : ISoftDeleteable
{
    public long Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public DateTime? FechaEliminacion { get; init; }

    public ICollection<Usuario> Usuarios { get; init; } = [];
    public ICollection<CuentaCategoria>Categorias { get; init; } = [];
}
