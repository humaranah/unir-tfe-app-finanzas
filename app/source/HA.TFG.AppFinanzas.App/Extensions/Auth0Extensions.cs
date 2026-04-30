using Auth0.OidcClient;
using HA.TFG.AppFinanzas.App.Authentication;
using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Extensions;

internal static class Auth0Extensions
{
    private const string RedirectUri = "ha.tfg.appfinanzas://callback";

    internal static MauiAppBuilder AddAuth0(this MauiAppBuilder builder)
    {
        var domain = builder.Configuration["Auth0:Domain"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(domain))
            throw new InvalidOperationException("Auth0:Domain is not configured.");

        var clientId = builder.Configuration["Auth0:ClientId"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Auth0:ClientId is not configured.");

        var audience = builder.Configuration["Auth0:Audience"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Auth0:Audience is not configured.");

#if WINDOWS
        builder.Services.AddSingleton<IAuth0Client>(new AudienceAwareAuth0Client(
            new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId,
                RedirectUri = RedirectUri,
                PostLogoutRedirectUri = RedirectUri,
                Scope = "openid profile email offline_access",
                Browser = new EmbeddedBrowser()
            }),
            audience));
#else
        builder.Services.AddSingleton<IAuth0Client>(new AudienceAwareAuth0Client(
            new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId,
                RedirectUri = RedirectUri,
                PostLogoutRedirectUri = RedirectUri,
                Scope = "openid profile email offline_access"
            }),
            audience));
#endif

        builder.Services.AddSingleton<ISessionStore, SecureSessionStore>();

        return builder;
    }
}
