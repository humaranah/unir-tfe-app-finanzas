using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientoDetalleQuery;

public sealed class GetMovimientoDetalleQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository)
    : IRequestHandler<GetMovimientoDetalleQuery, GetMovimientoDetalleResult>
{
    public async ValueTask<GetMovimientoDetalleResult> Handle(
        GetMovimientoDetalleQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta);

        var movimiento = await movimientoRepository.GetMovimientoByIdAsync(cuenta.IdCuenta, request.IdMovimiento, cancellationToken)
            ?? throw new NotFoundException(nameof(Movimiento), request.IdMovimiento);

        return movimiento.ToResult();
    }
}
