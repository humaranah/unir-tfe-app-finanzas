using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using HA.TFG.AppFinanzas.BackEnd.Mappers;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    // POST api/usuarios/ensure
    // La aplicación llama a este endpoint tras el login y el IdAuth0 se obtiene del JWT (claim "sub").
    [HttpPost("ensure")]
    [Authorize]
    public async Task<IActionResult> Ensure(
        [FromBody] EnsureUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var idAuth0 = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(idAuth0))
            return BadRequest("El token no contiene el claim 'sub' requerido.");

        var command = request.ToEnsureUsuarioCommand() with { IdAuth0 = idAuth0 };

        var result = await _mediator.Send(command, cancellationToken);

        return result.EsNuevo
            ? CreatedAtAction(nameof(Ensure), new { id = result.Id }, result)
            : Ok(result);
    }
}
