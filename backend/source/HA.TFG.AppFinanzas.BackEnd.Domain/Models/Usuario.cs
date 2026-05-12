using HA.TFG.AppFinanzas.BackEnd.Domain.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Usuario : IAuditable, ISoftDeleteable
{
    public Guid IdUsuario { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? FotoPerfil { get; init; }
    public bool EmailVerificado { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public DateTime? FechaEliminacion { get; init; }

    public ICollection<Identidad> Identidades { get; init; } = [];
    public ICollection<Rol> Roles { get; init; } = [];
    public ICollection<Cuenta> Cuentas { get; init; } = [];
}
