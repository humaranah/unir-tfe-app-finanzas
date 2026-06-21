using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;

public class UpdateMovimientoCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    ICuentaCategoriaRepository cuentaCategoriaRepository,
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

        _ = await cuentaCategoriaRepository.GetCategoriaByIdAsync(request.IdCuenta, request.IdCuentaCategoria, cancellationToken)
            ?? throw new NotFoundException(nameof(CuentaCategoria), request.IdCuentaCategoria.ToString());

        var newComprobanteId = await UploadComprobanteAsync(cuenta.IdCuenta, request, cancellationToken);
        var movimientoActualizado = BuildUpdatedMovimiento(movimiento, request, newComprobanteId);

        try
        {
            var resultado = await movimientoRepository.UpdateMovimientoAsync(movimientoActualizado, cancellationToken);

            await DeletePreviousComprobanteAsync(cuenta.IdCuenta, movimiento.IdComprobante, newComprobanteId);

            return resultado.ToResult();
        }
        catch (Exception ex)
        {
            await DeleteOrphanedComprobanteAsync(cuenta.IdCuenta, newComprobanteId, ex);

            throw;
        }
    }

    private async ValueTask<string?> UploadComprobanteAsync(Guid idCuenta, UpdateMovimientoCommand request, CancellationToken cancellationToken)
    {
        if (request.ComprobanteStream is null
            || string.IsNullOrWhiteSpace(request.ComprobanteFileName)
            || string.IsNullOrWhiteSpace(request.ComprobanteContentType))
        {
            return null;
        }

        var uploadedId = await comprobanteStorage.UploadComprobanteAsync(
            idCuenta,
            request.ComprobanteFileName,
            request.ComprobanteContentType,
            request.ComprobanteStream,
            cancellationToken);

        if (!string.IsNullOrEmpty(uploadedId))
        {
            return uploadedId;
        }

        logger.LogWarning(
            "No se pudo subir el nuevo comprobante para el movimiento {IdMovimiento}. Continuando sin actualizar el comprobante.",
            request.IdMovimiento);

        return null;
    }

    private static Movimiento BuildUpdatedMovimiento(Movimiento movimiento, UpdateMovimientoCommand request, string? newComprobanteId)
    {
        if (string.IsNullOrEmpty(newComprobanteId))
        {
            newComprobanteId = movimiento.IdComprobante;
        }

        return movimiento with
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
            IdComprobante = newComprobanteId,
            FechaModificacion = DateTime.UtcNow,
            Categoria = null
        };
    }

    private async Task DeletePreviousComprobanteAsync(Guid idCuenta, string? previousComprobanteId, string? newComprobanteId)
    {
        if (newComprobanteId is null || previousComprobanteId is null)
        {
            return;
        }

        try
        {
            await comprobanteStorage.DeleteComprobanteAsync(idCuenta, previousComprobanteId, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "No se pudo eliminar el comprobante anterior: {IdComprobante}", previousComprobanteId);
        }
    }

    private async Task DeleteOrphanedComprobanteAsync(Guid idCuenta, string? newComprobanteId, Exception exception)
    {
        if (newComprobanteId is null)
        {
            return;
        }

        logger.LogWarning(exception,
            "Error al persistir el movimiento. Eliminando comprobante huérfano: {IdComprobante}", newComprobanteId);

        await comprobanteStorage.DeleteComprobanteAsync(idCuenta, newComprobanteId, CancellationToken.None);
    }
}
