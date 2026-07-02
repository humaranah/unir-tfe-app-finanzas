namespace HA.TFG.AppFinanzas.Core.Features.Authentication;

public interface IBackendHealthService
{
    /// <summary>
    /// Devuelve true si el servidor backend está disponible.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
