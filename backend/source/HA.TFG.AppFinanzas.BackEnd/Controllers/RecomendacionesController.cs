using HA.TFG.AppFinanzas.BackEnd.Application.Features.Recomendaciones.ObtenerRecomendacionesQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Roles.Usuario)]
public sealed class RecomendacionesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Genera recomendaciones financieras personalizadas basadas en los gastos del mes actual
    /// y el historial de meses anteriores. Acepta una consulta adicional opcional del usuario.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<RecomendacionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerRecomendaciones(
        [FromBody] ObtenerRecomendacionesRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta   = request.IdCuenta,
            UserEmail  = User.Identity?.Name ?? string.Empty,
            Query      = request.Query
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
