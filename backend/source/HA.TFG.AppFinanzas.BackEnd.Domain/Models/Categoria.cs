namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Categoria
{
    public long Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
}
