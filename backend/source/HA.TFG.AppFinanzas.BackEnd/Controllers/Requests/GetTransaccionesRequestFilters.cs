using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record GetTransaccionesRequestFilters
{
    [FromQuery(Name = "idCategoria")]
    public long? IdCategoria { get; init; }

    [FromQuery(Name = "fechaDesde")]
    public DateOnly? FechaDesde { get; init; }

    [FromQuery(Name = "fechaHasta")]
    public DateOnly? FechaHasta { get; init; }
}
