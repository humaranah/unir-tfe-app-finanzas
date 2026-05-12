using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record Categoria
{
    public Guid IdCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }

    public static Categoria CrearNuevo(TipoMovimiento tipoMovimiento, string nombre, string descripcion) =>
        new()
        {
            IdCategoria = Guid.CreateVersion7(),
            TipoMovimiento = tipoMovimiento,
            Nombre = nombre,
            Descripcion = descripcion,
            FechaCreacion = DateTime.UtcNow
        };
}
