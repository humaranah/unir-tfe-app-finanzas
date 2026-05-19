using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;

public record GetCuentaCategoriasResultItem
{
    public Guid IdCuentaCategoria { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public TipoMovimiento TipoMovimiento { get; init; }
}
