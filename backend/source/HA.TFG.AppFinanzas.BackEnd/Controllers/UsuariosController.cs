using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;
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
    // La aplicación llama a este endpoint tras el login.
    // El IdAuth0 se obtiene del claim "sub" del JWT y los datos de perfil se obtienen
    // directamente desde Auth0 /userinfo (solo si el usuario no existe aún).
    [HttpPost("ensure")]
    [Authorize]
    public async Task<IActionResult> Ensure(CancellationToken cancellationToken)
    {
        var idAuth0 = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(idAuth0))
            return BadRequest("Ha ocurrido un error validando la información del usuario.");

        var accessToken = Request.Headers.Authorization.ToString();
        if (accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            accessToken = accessToken["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(accessToken))
            return BadRequest("No se encontró el token de acceso en la solicitud.");

        var command = new EnsureUsuarioCommand(idAuth0, accessToken);

        var result = await _mediator.Send(command, cancellationToken);

        return result.EsNuevo
            ? CreatedAtAction(nameof(Ensure), new { id = result.Id }, result)
            : Ok(result);
    }
}
