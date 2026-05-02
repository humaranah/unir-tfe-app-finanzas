namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface IUsuarioSyncService
{
    Task SyncUsuarioAsync(UsuarioInfo usuario, CancellationToken cancellationToken = default);
}