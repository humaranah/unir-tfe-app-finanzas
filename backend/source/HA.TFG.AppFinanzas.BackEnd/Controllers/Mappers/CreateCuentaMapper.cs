using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class CreateCuentaMapper
{
    public static CreateCuentaCommand ToCommand(this CreateCuentaRequest request, string email) =>
        ToCommandInternal(request) with { Email = email };

    [MapperIgnoreTarget(nameof(CreateCuentaCommand.Email))]
    private static partial CreateCuentaCommand ToCommandInternal(CreateCuentaRequest request);
}
