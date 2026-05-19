using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetComprobanteMovimientoQuery;

public sealed class GetComprobanteMovimientoQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository,
    IComprobanteStorageService comprobanteStorageService)
    : IRequestHandler<GetComprobanteMovimientoQuery, ComprobanteFile>
{
    public async ValueTask<ComprobanteFile> Handle(
        GetComprobanteMovimientoQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta);

        var movimiento = await movimientoRepository.GetMovimientoByIdAsync(cuenta.IdCuenta, request.IdMovimiento, cancellationToken)
            ?? throw new NotFoundException(nameof(Movimiento), request.IdMovimiento);

        if (string.IsNullOrWhiteSpace(movimiento.IdComprobante))
            throw new NotFoundException("Comprobante", request.IdMovimiento);

        return await comprobanteStorageService.GetComprobanteAsync(cuenta.IdCuenta, movimiento.IdComprobante, cancellationToken);
    }
}
