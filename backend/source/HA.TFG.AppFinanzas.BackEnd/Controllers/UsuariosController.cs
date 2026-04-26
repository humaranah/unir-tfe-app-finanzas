using HA.TFG.AppFinanzas.BackEnd.Mappers;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController(IMediator mediator) : ControllerBase
{
    // POST api/usuarios/sync
    // MAUI llama a este endpoint tras el login — lee los datos del JWT directamente
    [HttpPost("sync")]
    [Authorize]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var command = User.ToSyncUsuarioCommand();

        if (command is null)
            return BadRequest("El token no contiene los claims 'sub', 'email' o 'name' requeridos.");

        var result = await mediator.Send(command, cancellationToken);

        return result.EsNuevo
            ? CreatedAtAction(nameof(Sync), new { id = result.Id }, result)
            : Ok(result);
    }
}
