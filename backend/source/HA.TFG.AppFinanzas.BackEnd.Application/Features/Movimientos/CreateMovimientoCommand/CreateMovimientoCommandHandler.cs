using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;

public class CreateMovimientoCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository,
    IComprobanteStorageService comprobanteStorage,
    ILogger<CreateMovimientoCommandHandler> logger)
    : IRequestHandler<CreateMovimientoCommand, CreateMovimientoResult>
{
    public async ValueTask<CreateMovimientoResult> Handle(CreateMovimientoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta.ToString());

        // Validar que la categoría pertenece a la cuenta
        _ = await cuentaRepository.GetCategoriaByIdAsync(cuenta.IdCuenta, request.IdCuentaCategoria, cancellationToken)
            ?? throw new NotFoundException(nameof(CuentaCategoria), request.IdCuentaCategoria.ToString());

        // Subir comprobante antes de persistir en BD
        string? idComprobante = null;
        if (request.ComprobanteStream is not null
            && !string.IsNullOrWhiteSpace(request.ComprobanteFileName)
            && !string.IsNullOrWhiteSpace(request.ComprobanteContentType))
        {
            idComprobante = await comprobanteStorage.UploadComprobanteAsync(
                cuenta.IdCuenta,
                request.ComprobanteFileName,
                request.ComprobanteContentType,
                request.ComprobanteStream,
                cancellationToken);
        }

        try
        {
            var movimiento = request.ToMovimiento() with
            {
                IdCuenta = cuenta.IdCuenta,
                IdCuentaCategoria = request.IdCuentaCategoria,
                IdComprobante = idComprobante,
                FechaCreacion = DateTime.UtcNow
            };

            var resultado = await movimientoRepository.AddMovimientoAsync(movimiento, cancellationToken);

            return resultado.ToResult();
        }
        catch (Exception ex)
        {
            // Rollback: eliminar el archivo subido si la BD falla
            if (idComprobante is not null)
            {
                logger.LogWarning(ex,
                    "Error al persistir el movimiento. Eliminando comprobante huérfano: {IdComprobante}", idComprobante);

                await comprobanteStorage.DeleteComprobanteAsync(cuenta.IdCuenta, idComprobante, CancellationToken.None);
            }

            throw;
        }
    }
}
