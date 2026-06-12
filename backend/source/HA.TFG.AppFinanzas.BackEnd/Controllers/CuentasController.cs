using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.DeleteMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetComprobanteMovimientoQuery;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientoDetalleQuery;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientosQuery;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;
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

    [HttpGet("{idCuenta:guid}/movimientos")]
    [ProducesResponseType<IReadOnlyList<GetMovimientosResultItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovimientos(
        [FromRoute] Guid idCuenta,
        [FromQuery] GetMovimientosRequestFilters filters,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var query = filters.ToQuery(idCuenta, email);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{idCuenta:guid}/categorias")]
    [ProducesResponseType<IReadOnlyList<GetCuentaCategoriasResultItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoriasCuenta(
        [FromRoute] Guid idCuenta,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var result = await _mediator.Send(new GetCuentaCategoriasQuery(email, idCuenta), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<CreateCuentaResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCuenta([FromBody] CreateCuentaRequest request, CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var command = request.ToCommand(email);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{idCuenta:guid}/movimientos/{idMovimiento:guid}")]
    [ProducesResponseType<GetMovimientoDetalleResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMovimientoDetalle(
        [FromRoute] Guid idCuenta,
        [FromRoute] Guid idMovimiento,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var result = await _mediator.Send(new GetMovimientoDetalleQuery(email, idCuenta, idMovimiento), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{idCuenta:guid}/movimientos/{idMovimiento:guid}/comprobante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComprobanteMovimiento(
        [FromRoute] Guid idCuenta,
        [FromRoute] Guid idMovimiento,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var comprobante = await _mediator.Send(new GetComprobanteMovimientoQuery(email, idCuenta, idMovimiento), cancellationToken);
        return File(comprobante.Stream, comprobante.ContentType, comprobante.FileName);
    }

    [HttpPost("{idCuenta:guid}/movimientos")]
    [ProducesResponseType<CreateMovimientoResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMovimiento(
        [FromRoute] Guid idCuenta,
        [FromForm] CreateMovimientoRequest request,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var command = request.ToCommand(email, idCuenta);

        if (request.Comprobante is { Length: > 0 } archivo)
        {
            command = command with
            {
                ComprobanteStream = archivo.OpenReadStream(),
                ComprobanteFileName = archivo.FileName,
                ComprobanteContentType = archivo.ContentType
            };
        }

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetMovimientos), new { idCuenta }, result);
        }
        finally
        {
            if (command.ComprobanteStream != null)
            {
                await command.ComprobanteStream.DisposeAsync();
            }
        }
    }

    [HttpPut("{idCuenta:guid}/movimientos/{idMovimiento:guid}")]
    [ProducesResponseType<UpdateMovimientoResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMovimiento(
        [FromRoute] Guid idCuenta,
        [FromRoute] Guid idMovimiento,
        [FromForm] UpdateMovimientoRequest request,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var command = request.ToCommand(email, idCuenta, idMovimiento);

        if (request.Comprobante is { Length: > 0 } archivo)
        {
            command = command with
            {
                ComprobanteStream = archivo.OpenReadStream(),
                ComprobanteFileName = archivo.FileName,
                ComprobanteContentType = archivo.ContentType
            };
        }

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        finally
        {
            if (command.ComprobanteStream != null)
            {
                await command.ComprobanteStream.DisposeAsync();
            }
        }
    }

    [HttpDelete("{idCuenta:guid}/movimientos/{idMovimiento:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovimiento(
        [FromRoute] Guid idCuenta,
        [FromRoute] Guid idMovimiento,
        CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? string.Empty;
        var command = new DeleteMovimientoCommand
        {
            Email = email,
            IdCuenta = idCuenta,
            IdMovimiento = idMovimiento
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
