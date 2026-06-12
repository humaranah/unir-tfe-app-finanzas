using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.DeleteMovimientoCommand;

public class DeleteMovimientoCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository,
    IComprobanteStorageService comprobanteStorage,
    ILogger<DeleteMovimientoCommandHandler> logger)
    : IRequestHandler<DeleteMovimientoCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteMovimientoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);
        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta.ToString());
        var movimiento = await movimientoRepository.GetMovimientoByIdAsync(request.IdCuenta, request.IdMovimiento, cancellationToken)
            ?? throw new NotFoundException(nameof(Movimiento), request.IdMovimiento.ToString());

        await movimientoRepository.DeleteMovimientoAsync(movimiento, cancellationToken);

        try
        {
            await DeleteComprobanteAsync(cuenta.IdCuenta, movimiento.IdComprobante);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "No se pudo eliminar el comprobante asociado al movimiento: {IdComprobante}", movimiento.IdComprobante);
        }

        return Unit.Value;
    }

    private async Task DeleteComprobanteAsync(Guid idCuenta, string? comprobanteId)
    {
        if (string.IsNullOrEmpty(comprobanteId))
        {
            return;
        }

        await comprobanteStorage.DeleteComprobanteAsync(idCuenta, comprobanteId, CancellationToken.None);
    }
}
