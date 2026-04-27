using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Authentication;

internal sealed class SecureSessionStore : ISessionStore
{
    private const string Key = "auth0_refresh_token";

    public Task SaveRefreshTokenAsync(string refreshToken) =>
        SecureStorage.Default.SetAsync(Key, refreshToken);

    public Task<string?> LoadRefreshTokenAsync() =>
        SecureStorage.Default.GetAsync(Key)!;

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(Key);
        return Task.CompletedTask;
    }
}
