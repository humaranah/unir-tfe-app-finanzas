using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Transacciones.GetTransaccionesQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Mappers;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Roles.Usuario)]
public sealed class CuentasController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetCuentas(CancellationToken cancellationToken)
    {
        var query = new GetCuentasQuery(User.Identity?.Name ?? string.Empty);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{idCuenta:long}/transacciones")]
    [ProducesResponseType<IReadOnlyList<GetTransaccionesResultItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransacciones(
        [FromRoute] long idCuenta,
        [FromQuery] GetTransaccionesRequestFilters filters,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var query = filters.ToQuery(idCuenta, email);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<CreateCuentaResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCuenta([FromBody] CreateCuentaRequest request, CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var command = request.ToCommand(email);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
