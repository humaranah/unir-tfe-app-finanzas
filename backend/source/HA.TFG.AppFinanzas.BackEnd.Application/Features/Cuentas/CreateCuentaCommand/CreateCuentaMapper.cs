using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class CreateCuentaMapper
{
    [MapProperty(nameof(Cuenta.Id), nameof(CreateCuentaResult.IdCuenta))]
    public static partial CreateCuentaResult ToResult(this Cuenta cuenta);
}
