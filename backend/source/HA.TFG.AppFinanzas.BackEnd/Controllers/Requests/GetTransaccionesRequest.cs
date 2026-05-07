using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record GetTransaccionesRequest
{
    [FromRoute(Name = "idCuenta")]
    public required long IdCuenta { get; init; }

    [FromQuery(Name = "idCategoria")]
    public long? IdCategoria { get; init; }

    [FromQuery(Name = "fechaDesde")]
    public DateOnly? FechaDesde { get; init; }

    [FromQuery(Name = "fechaHasta")]
    public DateOnly? FechaHasta { get; init; }
}
