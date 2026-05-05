using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;

public class GetCuentasQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    ILogger<GetCuentasQueryHandler> logger)
    : IRequestHandler<GetCuentasQuery, IReadOnlyList<GetCuentasResultItem>>
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly ICuentaRepository _cuentaRepository = cuentaRepository;
    private readonly ILogger<GetCuentasQueryHandler> _logger = logger;

    public async ValueTask<IReadOnlyList<GetCuentasResultItem>> Handle(
        GetCuentasQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetCuentasQuery for email: {Email}", request.Email);

        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);
        if (usuario == null)
        {
            _logger.LogWarning("User not found for email: {Email}", request.Email);
            return [];
        }

        var cuentas = await _cuentaRepository.GetCuentasByUsuarioIdAsync(usuario.Id, cancellationToken)
            .ConfigureAwait(false);

        var resultItems = cuentas
            .Select(cuenta => cuenta.ToResultItem())
            .ToList();

        _logger.LogInformation("Retrieved {Count} accounts for user: {Email}", resultItems.Count, request.Email);
        return resultItems;
    }
}
