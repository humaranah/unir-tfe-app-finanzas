using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.UpdateCuentaCategoriaCommand;

public sealed class UpdateCuentaCategoriaCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    ICuentaCategoriaRepository cuentaCategoriaRepository)
    : IRequestHandler<UpdateCuentaCategoriaCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateCuentaCategoriaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        _ = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta.ToString());

        var categoria = await cuentaCategoriaRepository.GetCategoriaByIdAsync(request.IdCuenta, request.IdCuentaCategoria, cancellationToken)
            ?? throw new NotFoundException(nameof(CuentaCategoria), request.IdCuentaCategoria.ToString());

        var existente = await cuentaCategoriaRepository.GetCategoriaByNombreAsync(request.IdCuenta, request.Nombre, cancellationToken);
        if (existente is not null && existente.IdCuentaCategoria != request.IdCuentaCategoria)
            throw new ConflictException(nameof(CuentaCategoria), request.Nombre);

        var categoriaActualizada = categoria with
        {
            Nombre = request.Nombre,
            TipoMovimiento = request.TipoMovimiento,
            FechaModificacion = DateTime.UtcNow
        };

        await cuentaCategoriaRepository.UpdateCategoriaAsync(categoriaActualizada, cancellationToken);

        return Unit.Value;
    }
}
