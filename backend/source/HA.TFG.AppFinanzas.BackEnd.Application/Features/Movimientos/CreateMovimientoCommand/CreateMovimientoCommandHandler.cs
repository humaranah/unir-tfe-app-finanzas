using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;

public class CreateMovimientoCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository)
    : IRequestHandler<CreateMovimientoCommand, CreateMovimientoResult>
{
    public async ValueTask<CreateMovimientoResult> Handle(CreateMovimientoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta.ToString());

        var movimiento = new Movimiento
        {
            IdCuenta = cuenta.IdCuenta,
            IdCuentaCategoria = request.IdCuentaCategoria,
            TipoMovimiento = request.TipoMovimiento,
            Concepto = request.Concepto,
            Importe = request.Importe,
            Moneda = request.Moneda,
            TipoCambio = request.TipoCambio,
            IdComprobante = request.IdComprobante,
            Nota = request.Nota,
            FechaMovimiento = request.FechaMovimiento,
            FechaCreacion = DateTime.UtcNow
        };

        var resultado = await movimientoRepository.AddMovimientoAsync(movimiento, cancellationToken);

        return new CreateMovimientoResult
        {
            IdMovimiento = resultado.IdMovimiento,
            IdCuenta = resultado.IdCuenta,
            IdCuentaCategoria = resultado.IdCuentaCategoria,
            TipoMovimiento = resultado.TipoMovimiento,
            Concepto = resultado.Concepto,
            Importe = resultado.Importe,
            Moneda = resultado.Moneda,
            TipoCambio = resultado.TipoCambio,
            IdComprobante = resultado.IdComprobante,
            Nota = resultado.Nota,
            FechaMovimiento = resultado.FechaMovimiento
        };
    }
}
