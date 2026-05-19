using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientoDetalleQuery;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class GetMovimientoDetalleMapper
{
    [MapProperty(nameof(Movimiento.Categoria.Nombre), nameof(GetMovimientoDetalleResult.NombreCategoria))]
    public static partial GetMovimientoDetalleResult ToResult(this Movimiento movimiento);
}
