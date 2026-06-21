using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCategoriaCommand;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class CuentaCategoriaMapper
{
    public static CreateCuentaCategoriaCommand ToCreateCommand(
        this CreateCuentaCategoriaRequest request,
        string email,
        Guid idCuenta) =>
        ToCreateCommandInternal(request) with { Email = email, IdCuenta = idCuenta };

    [MapperIgnoreTarget(nameof(CreateCuentaCategoriaCommand.Email))]
    [MapperIgnoreTarget(nameof(CreateCuentaCategoriaCommand.IdCuenta))]
    private static partial CreateCuentaCategoriaCommand ToCreateCommandInternal(CreateCuentaCategoriaRequest request);
}
