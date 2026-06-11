using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;

public class UpdateMovimientoCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository,
    IComprobanteStorageService comprobanteStorage,
    ILogger<UpdateMovimientoCommandHandler> logger)
    : IRequestHandler<UpdateMovimientoCommand, UpdateMovimientoResult>
{
    public async ValueTask<UpdateMovimientoResult> Handle(UpdateMovimientoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta.ToString());

        var movimiento = await movimientoRepository.GetMovimientoByIdAsync(request.IdCuenta, request.IdMovimiento, cancellationToken)
            ?? throw new NotFoundException(nameof(Movimiento), request.IdMovimiento.ToString());

        // Validar que la categoría pertenece a la cuenta
        _ = await cuentaRepository.GetCategoriaByIdAsync(cuenta.IdCuenta, request.IdCuentaCategoria, cancellationToken)
            ?? throw new NotFoundException(nameof(CuentaCategoria), request.IdCuentaCategoria.ToString());

        // Subir nuevo comprobante antes de persistir en BD
        string? nuevoIdComprobante = null;
        if (request.ComprobanteStream is not null
            && !string.IsNullOrWhiteSpace(request.ComprobanteFileName)
            && !string.IsNullOrWhiteSpace(request.ComprobanteContentType))
        {
            nuevoIdComprobante = await comprobanteStorage.UploadComprobanteAsync(
                cuenta.IdCuenta,
                request.ComprobanteFileName,
                request.ComprobanteContentType,
                request.ComprobanteStream,
                cancellationToken);
        }

        var movimientoActualizado = movimiento with
        {
            IdCuentaCategoria = request.IdCuentaCategoria,
            TipoMovimiento = request.TipoMovimiento,
            Concepto = request.Concepto,
            Establecimiento = request.Establecimiento,
            Importe = request.Importe,
            Moneda = request.Moneda,
            TipoCambio = request.TipoCambio,
            Nota = request.Nota,
            FechaMovimiento = request.FechaMovimiento,
            IdComprobante = nuevoIdComprobante ?? movimiento.IdComprobante,
            FechaModificacion = DateTime.UtcNow,
            Categoria = null
        };

        try
        {
            var resultado = await movimientoRepository.UpdateMovimientoAsync(movimientoActualizado, cancellationToken);

            // Eliminar el comprobante anterior si se reemplazó por uno nuevo
            if (nuevoIdComprobante is not null && movimiento.IdComprobante is not null)
            {
                try
                {
                    await comprobanteStorage.DeleteComprobanteAsync(cuenta.IdCuenta, movimiento.IdComprobante, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "No se pudo eliminar el comprobante anterior: {IdComprobante}", movimiento.IdComprobante);
                }
            }

            return resultado.ToResult();
        }
        catch (Exception ex)
        {
            if (nuevoIdComprobante is not null)
            {
                logger.LogWarning(ex,
                    "Error al persistir el movimiento. Eliminando comprobante huérfano: {IdComprobante}", nuevoIdComprobante);

                await comprobanteStorage.DeleteComprobanteAsync(cuenta.IdCuenta, nuevoIdComprobante, CancellationToken.None);
            }

            throw;
        }
    }
}
