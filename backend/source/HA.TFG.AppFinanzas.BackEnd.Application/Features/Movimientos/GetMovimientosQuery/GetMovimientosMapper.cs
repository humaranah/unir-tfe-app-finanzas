using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientosQuery;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class GetMovimientosMapper
{
    [MapProperty([nameof(Movimiento.IdCuentaCategoria)], nameof(GetMovimientosResultItem.IdCategoria))]
    [MapProperty([nameof(Movimiento.Categoria), nameof(CuentaCategoria.Nombre)], nameof(GetMovimientosResultItem.NombreCategoria))]
    public static partial GetMovimientosResultItem ToResultItem(this Movimiento movimiento);

    private static DateOnly DateTimeToDateOnly(DateTime dateTime) => DateOnly.FromDateTime(dateTime);
    private static string? NombreCategoriaOrNull(string? nombre) => nombre;
}
