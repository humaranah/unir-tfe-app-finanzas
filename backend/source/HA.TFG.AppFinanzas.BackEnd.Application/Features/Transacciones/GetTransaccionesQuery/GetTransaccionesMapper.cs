using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Transacciones.GetTransaccionesQuery;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class GetTransaccionesMapper
{
    [MapProperty(nameof(Transaccion.Monto), nameof(GetTransaccionesResultItem.Importe))]
    [MapProperty(nameof(Transaccion.Descripcion), nameof(GetTransaccionesResultItem.Concepto))]
    [MapProperty([nameof(Transaccion.Categoria), nameof(CuentaCategoria.Id)], nameof(GetTransaccionesResultItem.IdCategoria))]
    [MapProperty([nameof(Transaccion.Categoria), nameof(CuentaCategoria.Nombre)], nameof(GetTransaccionesResultItem.NombreCategoria))]
    public static partial GetTransaccionesResultItem ToResultItem(this Transaccion transaccion);

    private static DateOnly DateTimeToDateOnly(DateTime dateTime) => DateOnly.FromDateTime(dateTime);
}
