using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Authentication;

internal sealed class SecureSessionStore : ISessionStore
{
    private const string RefreshTokenKey = "auth0_refresh_token";
    private const string AccessTokenKey = "auth0_access_token";

    public Task SaveRefreshTokenAsync(string refreshToken) =>
        SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);

    public Task<string?> LoadRefreshTokenAsync() =>
        SecureStorage.Default.GetAsync(RefreshTokenKey)!;

    public Task SaveAccessTokenAsync(string accessToken) =>
        SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);

    public Task<string?> LoadAccessTokenAsync() =>
        SecureStorage.Default.GetAsync(AccessTokenKey)!;

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(AccessTokenKey);
        return Task.CompletedTask;
    }
}
