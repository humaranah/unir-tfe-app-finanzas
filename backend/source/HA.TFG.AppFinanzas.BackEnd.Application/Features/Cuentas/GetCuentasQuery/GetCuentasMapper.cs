using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class GetCuentasMapper
{
    public static partial GetCuentasResultItem ToResultItem(this Cuenta cuenta);
}

