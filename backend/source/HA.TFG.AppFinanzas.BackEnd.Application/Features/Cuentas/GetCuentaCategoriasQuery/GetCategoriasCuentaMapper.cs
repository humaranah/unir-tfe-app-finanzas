using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class GetCuentaCategoriasMapper
{
    public static partial GetCuentaCategoriasResultItem ToResultItem(this CuentaCategoria cuentaCategoria);
}
