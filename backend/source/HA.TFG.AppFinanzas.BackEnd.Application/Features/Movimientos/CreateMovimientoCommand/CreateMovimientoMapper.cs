using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class CreateMovimientoMapper
{
    // CreateMovimientoCommand → Movimiento
    // IdCuenta se sobreescribe en el handler con el valor validado de la cuenta
    // IdComprobante y FechaCreacion los asigna el handler tras subir el archivo
    [MapperIgnoreTarget(nameof(Movimiento.IdMovimiento))]
    [MapperIgnoreTarget(nameof(Movimiento.IdCuentaCategoria))]
    [MapperIgnoreTarget(nameof(Movimiento.IdComprobante))]
    [MapperIgnoreTarget(nameof(Movimiento.FechaCreacion))]
    [MapperIgnoreTarget(nameof(Movimiento.FechaModificacion))]
    [MapperIgnoreTarget(nameof(Movimiento.FechaEliminacion))]
    [MapperIgnoreTarget(nameof(Movimiento.Cuenta))]
    [MapperIgnoreTarget(nameof(Movimiento.Categoria))]
    [MapperIgnoreSource(nameof(CreateMovimientoCommand.Email))]
    [MapperIgnoreSource(nameof(CreateMovimientoCommand.ComprobanteStream))]
    [MapperIgnoreSource(nameof(CreateMovimientoCommand.ComprobanteFileName))]
    [MapperIgnoreSource(nameof(CreateMovimientoCommand.ComprobanteContentType))]
    public static partial Movimiento ToMovimiento(this CreateMovimientoCommand command);

    // Movimiento → CreateMovimientoResult
    public static partial CreateMovimientoResult ToResult(this Movimiento movimiento);
}
