namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface IUsuarioSyncService
{
    Task EnsureUsuarioAsync(UsuarioInfo usuario, CancellationToken cancellationToken = default);
}