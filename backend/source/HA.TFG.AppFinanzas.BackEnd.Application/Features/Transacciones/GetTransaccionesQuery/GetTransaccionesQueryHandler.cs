using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Transacciones.GetTransaccionesQuery;

public class GetTransaccionesQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    ITransaccionRepository transaccionRepository,
    ILogger<GetTransaccionesQueryHandler> logger)
    : IRequestHandler<GetTransaccionesQuery, IReadOnlyList<GetTransaccionesResultItem>>
{
    public async ValueTask<IReadOnlyList<GetTransaccionesResultItem>> Handle(
        GetTransaccionesQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByEmailAsync(request.EmailUsuario, cancellationToken);
        if (usuario is null)
        {
            logger.LogWarning("Usuario no encontrado para email: {Email}", request.EmailUsuario);
            return [];
        }

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.Id, request.IdCuenta, cancellationToken);
        if (cuenta is null)
        {
            logger.LogWarning("Cuenta {IdCuenta} no encontrada para usuario {Email}", request.IdCuenta, request.EmailUsuario);
            return [];
        }

        var transacciones = await transaccionRepository.GetTransaccionesAsync(
            request.IdCuenta, request.IdCategoria, request.FechaDesde, request.FechaHasta, cancellationToken);

        logger.LogInformation("Recuperadas {Count} transacciones para cuenta {IdCuenta}", transacciones.Count, request.IdCuenta);

        return [.. transacciones.Select(t => t.ToResultItem())];
    }
}
