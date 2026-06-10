using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class UpdateMovimientoMapper
{
    public static UpdateMovimientoCommand ToCommand(
        this UpdateMovimientoRequest request,
        string email,
        Guid idCuenta,
        Guid idMovimiento) =>
        ToCommandInternal(request) with { Email = email, IdCuenta = idCuenta, IdMovimiento = idMovimiento };

    [MapperIgnoreTarget(nameof(UpdateMovimientoCommand.Email))]
    [MapperIgnoreTarget(nameof(UpdateMovimientoCommand.IdCuenta))]
    [MapperIgnoreTarget(nameof(UpdateMovimientoCommand.IdMovimiento))]
    private static partial UpdateMovimientoCommand ToCommandInternal(UpdateMovimientoRequest request);
}
