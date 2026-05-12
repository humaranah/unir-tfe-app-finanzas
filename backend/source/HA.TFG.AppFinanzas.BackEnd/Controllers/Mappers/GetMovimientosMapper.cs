using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientosQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;

public static class GetMovimientosMapper
{
    public static GetMovimientosQuery ToQuery(
        this GetMovimientosRequestFilters request,
        Guid idCuenta,
        string emailUsuario) =>
        new()
        {
            IdCuenta = idCuenta,
            EmailUsuario = emailUsuario,
            IdCategoria = request.IdCategoria,
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta
        };
}
