using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;

public sealed class GetCuentaCategoriasQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    ILogger<GetCuentaCategoriasQueryHandler> logger)
    : IRequestHandler<GetCuentaCategoriasQuery, IReadOnlyList<GetCuentaCategoriasResultItem>>
{
    public async ValueTask<IReadOnlyList<GetCuentaCategoriasResultItem>> Handle(
        GetCuentaCategoriasQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException("Usuario", request.Email);

        var categorias = await cuentaRepository.GetCategoriasByCuentaAsync(
            usuario.IdUsuario, request.IdCuenta, cancellationToken);

        if (categorias.Count == 0)
        {
            logger.LogWarning("Cuenta {IdCuenta} no encontrada para usuario {Email}", request.IdCuenta, request.Email);
            throw new NotFoundException("Cuenta", request.IdCuenta);
        }

        return [.. categorias.Select(c => c.ToResultItem())];
    }
}
