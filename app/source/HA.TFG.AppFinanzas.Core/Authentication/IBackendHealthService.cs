namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface IBackendHealthService
{
    /// <summary>
    /// Devuelve true si el servidor backend está disponible.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
