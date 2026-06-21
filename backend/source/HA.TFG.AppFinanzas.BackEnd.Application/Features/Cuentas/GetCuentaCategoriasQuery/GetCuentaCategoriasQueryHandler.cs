using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;

public sealed class GetCuentaCategoriasQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaCategoriaRepository cuentaCategoriaRepository)
    : IRequestHandler<GetCuentaCategoriasQuery, IReadOnlyList<GetCuentaCategoriasResultItem>>
{
    public async ValueTask<IReadOnlyList<GetCuentaCategoriasResultItem>> Handle(
        GetCuentaCategoriasQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        var categorias = await cuentaCategoriaRepository.GetCategoriasByCuentaAsync(
            usuario.IdUsuario, request.IdCuenta, cancellationToken);

        if (categorias.Count == 0)
            return [];

        return [.. categorias.Select(c => c.ToResultItem())];
    }
}
