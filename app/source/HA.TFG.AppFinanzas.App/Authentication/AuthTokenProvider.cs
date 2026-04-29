using Auth0.OidcClient;
using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Authentication;

internal sealed class AuthTokenProvider(IAuth0Client auth0Client, ISessionStore sessionStore) : ITokenProvider
{
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await sessionStore.LoadRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var result = await auth0Client.RefreshTokenAsync(refreshToken, cancellationToken);
        if (result.IsError)
            return null;

        if (!string.IsNullOrEmpty(result.RefreshToken))
            await sessionStore.SaveRefreshTokenAsync(result.RefreshToken);

        if (!string.IsNullOrEmpty(result.AccessToken))
            await sessionStore.SaveAccessTokenAsync(result.AccessToken);

        return result.AccessToken;
    }
}
