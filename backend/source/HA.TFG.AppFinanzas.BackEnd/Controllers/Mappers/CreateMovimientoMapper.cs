using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class CreateMovimientoMapper
{
    public static CreateMovimientoCommand ToCommand(
        this CreateMovimientoRequest request,
        string email,
        Guid idCuenta) =>
        ToCommandInternal(request) with { Email = email, IdCuenta = idCuenta };

    [MapperIgnoreTarget(nameof(CreateMovimientoCommand.Email))]
    [MapperIgnoreTarget(nameof(CreateMovimientoCommand.IdCuenta))]
    [MapperIgnoreTarget(nameof(CreateMovimientoCommand.ComprobanteStream))]
    [MapperIgnoreTarget(nameof(CreateMovimientoCommand.ComprobanteFileName))]
    [MapperIgnoreTarget(nameof(CreateMovimientoCommand.ComprobanteContentType))]
    [MapperIgnoreSource(nameof(CreateMovimientoRequest.Comprobante))]
    private static partial CreateMovimientoCommand ToCommandInternal(CreateMovimientoRequest request);
}
