namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record UsuarioRol
{
    public Guid IdUsuario { get; init; }
    public Guid IdRol { get; init; }
}
