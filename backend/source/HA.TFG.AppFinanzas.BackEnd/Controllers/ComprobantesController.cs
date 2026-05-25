using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Roles.Usuario)]
public sealed class ComprobantesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Escanea un comprobante (PDF o JPEG, máximo 1 MB) y extrae sus datos
    /// estructurados mediante OCR + IA.
    /// </summary>
    [HttpPost("escanear")]
    [RequestSizeLimit(1 * 1024 * 1024)] // 1 MB — límite a nivel de servidor, antes del buffering
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ComprobanteExtraidoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EscanearComprobante(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var query = new ProcesarComprobanteQuery
        {
            ComprobanteStream = stream,
            ContentType       = file.ContentType
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
