using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Core.Features.Cuentas;

public sealed record CategoriaFilaItem(CategoriaItem Categoria, bool IsEven)
{
    public Guid IdCuentaCategoria => Categoria.IdCuentaCategoria;
    public string Nombre => Categoria.Nombre;
    public TipoMovimiento TipoMovimiento => Categoria.TipoMovimiento;
}
