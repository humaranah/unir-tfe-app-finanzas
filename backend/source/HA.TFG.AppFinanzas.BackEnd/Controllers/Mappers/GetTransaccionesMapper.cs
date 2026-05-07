using HA.TFG.AppFinanzas.BackEnd.Application.Features.Transacciones.GetTransaccionesQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;

public static class GetTransaccionesMapper
{
    public static GetTransaccionesQuery ToQuery(this GetTransaccionesRequest request, string emailUsuario) =>
        new()
        {
            IdCuenta = request.IdCuenta,
            EmailUsuario = emailUsuario,
            IdCategoria = request.IdCategoria,
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta
        };
}
