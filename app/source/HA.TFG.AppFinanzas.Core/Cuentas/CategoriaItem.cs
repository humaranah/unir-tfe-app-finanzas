using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Core.Cuentas;

public sealed class CategoriaItem
{
    public Guid IdCuentaCategoria { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public TipoMovimiento TipoMovimiento { get; init; }
}
