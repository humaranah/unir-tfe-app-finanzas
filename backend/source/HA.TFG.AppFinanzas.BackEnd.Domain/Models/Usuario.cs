using HA.TFG.AppFinanzas.BackEnd.Domain.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Usuario : ISoftDeleteable
{
    public long Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? FotoPerfil { get; init; }
    public bool EmailVerificado { get; init; }
    public DateTimeOffset? UltimaActualizacion { get; init; }
    public string Metadata { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public DateTime? FechaEliminacion { get; init; }

    public ICollection<UsuarioIdentidad> Identidades { get; init; } = [];
    public ICollection<Rol> Roles { get; init; } = [];
    public ICollection<Cuenta> Cuentas { get; init; } = [];
}
