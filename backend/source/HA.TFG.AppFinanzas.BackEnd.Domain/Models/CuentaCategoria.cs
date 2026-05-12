using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Domain.Models;

public record CuentaCategoria : IAuditable, ISoftDeleteable
{
    public Guid IdCuentaCategoria { get; init; }
    public Guid IdCuenta { get; init; }
    public Guid? IdCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public DateTime? FechaEliminacion { get; init; }

    public Cuenta? Cuenta { get; init; }
    public Categoria? Origen { get; init; }
    public ICollection<Movimiento> Movimientos { get; init; } = [];
}
