namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record UsuarioCuenta
{
    public Guid IdUsuario { get; init; }
    public Guid IdCuenta { get; init; }
}
