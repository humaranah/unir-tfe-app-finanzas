using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientosQuery;

public class GetMovimientosQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository,
    ILogger<GetMovimientosQueryHandler> logger)
    : IRequestHandler<GetMovimientosQuery, IReadOnlyList<GetMovimientosResultItem>>
{
    public async ValueTask<IReadOnlyList<GetMovimientosResultItem>> Handle(
        GetMovimientosQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.EmailUsuario, cancellationToken);
        if (usuario is null)
        {
            logger.LogWarning("Usuario no encontrado para email: {Email}", request.EmailUsuario);
            return [];
        }

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken);
        if (cuenta is null)
        {
            logger.LogWarning("Cuenta {IdCuenta} no encontrada para usuario {Email}", request.IdCuenta, request.EmailUsuario);
            return [];
        }

        var movimientos = await movimientoRepository.GetMovimientosAsync(
            request.IdCuenta, request.IdCategoria, request.FechaDesde, request.FechaHasta, cancellationToken);

        logger.LogInformation("Recuperados {Count} movimientos para cuenta {IdCuenta}", movimientos.Count, request.IdCuenta);

        return [.. movimientos.Select(t => t.ToResultItem())];
    }
}
