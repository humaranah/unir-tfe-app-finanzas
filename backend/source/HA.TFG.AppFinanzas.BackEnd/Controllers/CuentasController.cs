using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;
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
}
