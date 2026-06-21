using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCategoriaCommand;

public sealed class CreateCuentaCategoriaCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    ICuentaCategoriaRepository cuentaCategoriaRepository)
    : IRequestHandler<CreateCuentaCategoriaCommand, Unit>
{
    public async ValueTask<Unit> Handle(CreateCuentaCategoriaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        _ = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta.ToString());

        var existente = await cuentaCategoriaRepository.GetCategoriaByNombreAsync(request.IdCuenta, request.Nombre, cancellationToken);
        if (existente is not null)
            throw new ConflictException(nameof(CuentaCategoria), request.Nombre);

        var categoria = new CuentaCategoria
        {
            IdCuenta = request.IdCuenta,
            Nombre = request.Nombre,
            TipoMovimiento = request.TipoMovimiento,
            FechaCreacion = DateTime.UtcNow
        };

        await cuentaCategoriaRepository.CreateCategoriaAsync(categoria, cancellationToken);

        return Unit.Value;
    }
}
