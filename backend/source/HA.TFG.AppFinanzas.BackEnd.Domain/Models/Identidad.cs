namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Identidad
{
    public Guid IdIdentidad { get; init; }
    public Guid IdUsuario { get; init; }
    public string IdAuth0 { get; init; } = string.Empty;
    public string? Proveedor { get; init; }
}
