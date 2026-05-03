namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record UsuarioIdentidad
{
    public long Id { get; init; }
    public string IdAuth0 { get; init; } = string.Empty;
    public string? Proveedor { get; init; }
    public long IdUsuario { get; init; }
}
