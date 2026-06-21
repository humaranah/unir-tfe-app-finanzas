using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.DeleteCuentaCategoriaCommand;

public sealed class DeleteCuentaCategoriaCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    ICuentaCategoriaRepository cuentaCategoriaRepository)
    : IRequestHandler<DeleteCuentaCategoriaCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteCuentaCategoriaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        _ = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta.ToString());

        var categoria = await cuentaCategoriaRepository.GetCategoriaByIdAsync(request.IdCuenta, request.IdCuentaCategoria, cancellationToken)
            ?? throw new NotFoundException(nameof(CuentaCategoria), request.IdCuentaCategoria.ToString());

        await cuentaCategoriaRepository.DeleteCategoriaAsync(categoria, cancellationToken);

        return Unit.Value;
    }
}
