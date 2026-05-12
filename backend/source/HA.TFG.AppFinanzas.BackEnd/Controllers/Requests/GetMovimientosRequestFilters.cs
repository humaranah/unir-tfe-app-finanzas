using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record GetMovimientosRequestFilters
{
    [FromQuery(Name = "idCategoria")]
    public Guid? IdCategoria { get; init; }

    [FromQuery(Name = "fechaDesde")]
    public DateOnly? FechaDesde { get; init; }

    [FromQuery(Name = "fechaHasta")]
    public DateOnly? FechaHasta { get; init; }
}
