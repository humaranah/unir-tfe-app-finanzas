using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

public class CreateCuentaCommandHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository)
    : IRequestHandler<CreateCuentaCommand, CreateCuentaResult>
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly ICuentaRepository _cuentaRepository = cuentaRepository;

    public async ValueTask<CreateCuentaResult> Handle(CreateCuentaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        var cuenta = new Cuenta
        {
            Moneda = request.Moneda,
            Descripcion = request.Descripcion,
            Usuarios = [usuario]
        };

        var resultado = await _cuentaRepository.CreateCuentaWithCategoriasAsync(cuenta, cancellationToken);

        return resultado.ToResult();
    }
}
