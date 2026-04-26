using HA.TFG.AppFinanzas.BackEnd.Domain.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Usuario : ISoftDeleteable
{
    public long Id { get; init; }
    public string IdAuth0 { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Metadata { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public DateTime? FechaEliminacion { get; init; }

    public ICollection<Rol> Roles { get; init; } = [];
    public ICollection<Cuenta> Cuentas { get; init; } = [];
}
