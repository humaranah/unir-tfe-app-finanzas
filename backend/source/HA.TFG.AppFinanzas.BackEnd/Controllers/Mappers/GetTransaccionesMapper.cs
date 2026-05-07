using HA.TFG.AppFinanzas.BackEnd.Application.Features.Transacciones.GetTransaccionesQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;

public static class GetTransaccionesMapper
{
    public static GetTransaccionesQuery ToQuery(
        this GetTransaccionesRequestFilters request,
        long idCuenta,
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
