using HA.TFG.AppFinanzas.BackEnd.Application;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Roles.Usuario)]
public sealed class ComprobantesController(IMediator mediator, IOptions<ComprobanteConfig> comprobanteOptions) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ComprobanteConfig _comprobanteConfig = comprobanteOptions.Value;

    /// <summary>
    /// Escanea un comprobante (PDF o JPEG) y extrae sus datos estructurados mediante OCR + IA.
    /// </summary>
    [HttpPost("scan")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ComprobanteExtraidoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EscanearComprobante(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        // Límite de tamaño aplicado en tiempo de ejecución desde configuración
        var sizeFeature = HttpContext?.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if (sizeFeature is { IsReadOnly: false })
            sizeFeature.MaxRequestBodySize = _comprobanteConfig.MaxSizeBytes;

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
