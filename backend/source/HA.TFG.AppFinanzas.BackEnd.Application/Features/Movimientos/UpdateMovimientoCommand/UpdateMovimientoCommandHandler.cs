using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;

public class UpdateMovimientoCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository)
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
            FechaModificacion = DateTime.UtcNow,
            Categoria = null
        };

        var resultado = await movimientoRepository.UpdateMovimientoAsync(movimientoActualizado, cancellationToken);

        return resultado.ToResult();
    }
}
