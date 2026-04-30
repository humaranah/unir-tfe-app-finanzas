using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController(IMediator mediator) : ControllerBase
{
    // POST api/usuarios/ensure
    // MAUI llama a este endpoint tras el login — el IdAuth0 se obtiene del JWT (claim "sub")
    // y el resto del perfil se recibe en el body.
    [HttpPost("ensure")]
    [Authorize]
    public async Task<IActionResult> Ensure(
        [FromBody] EnsureUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var idAuth0 = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(idAuth0))
            return BadRequest("El token no contiene el claim 'sub' requerido.");

        var command = new EnsureUsuarioCommand(
            IdAuth0: idAuth0,
            Email: request.Email,
            Nombre: request.Nombre,
            FotoPerfil: request.FotoPerfil,
            Proveedor: request.Proveedor,
            EmailVerificado: request.EmailVerificado,
            UltimaActualizacion: request.UltimaActualizacion);

        var result = await mediator.Send(command, cancellationToken);

        return result.EsNuevo
            ? CreatedAtAction(nameof(Ensure), new { id = result.Id }, result)
            : Ok(result);
    }
}
