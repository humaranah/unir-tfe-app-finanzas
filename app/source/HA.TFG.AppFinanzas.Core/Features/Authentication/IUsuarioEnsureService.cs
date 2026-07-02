namespace HA.TFG.AppFinanzas.Core.Features.Authentication;

public interface IUsuarioEnsureService
{
    Task EnsureUsuarioAsync(CancellationToken cancellationToken = default);
}