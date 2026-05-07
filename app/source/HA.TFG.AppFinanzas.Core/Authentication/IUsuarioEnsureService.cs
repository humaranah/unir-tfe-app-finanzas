namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface IUsuarioEnsureService
{
    Task EnsureUsuarioAsync(CancellationToken cancellationToken = default);
}