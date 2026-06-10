using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class UpdateMovimientoMapper
{
    // Movimiento → UpdateMovimientoResult
    public static partial UpdateMovimientoResult ToResult(this Movimiento movimiento);
}
