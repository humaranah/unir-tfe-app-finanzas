using Auth0.OidcClient;
using HA.TFG.AppFinanzas.App.Authentication;
using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Extensions;

internal static class Auth0Extensions
{
    private const string RedirectUri = "ha.tfg.appfinanzas://callback";

    internal static MauiAppBuilder AddAuth0(this MauiAppBuilder builder)
    {
        var domain = builder.Configuration["Auth0:Domain"]
            ?? throw new InvalidOperationException("Auth0:Domain is not configured.");
        var clientId = builder.Configuration["Auth0:ClientId"]
            ?? throw new InvalidOperationException("Auth0:ClientId is not configured.");

#if WINDOWS
        var browser = new EmbeddedBrowser();
        builder.Services.AddSingleton<IBrowserCookieCleaner>(browser);
        builder.Services.AddSingleton<IAuth0Client>(new Auth0Client(new Auth0ClientOptions
        {
            Domain = domain,
            ClientId = clientId,
            RedirectUri = RedirectUri,
            PostLogoutRedirectUri = RedirectUri,
            Scope = "openid profile email offline_access",
            Browser = browser
        }));
#else
        builder.Services.AddSingleton<IBrowserCookieCleaner>(new NullBrowserCookieCleaner());
        builder.Services.AddSingleton<IAuth0Client>(new Auth0Client(new Auth0ClientOptions
        {
            Domain = domain,
            ClientId = clientId,
            RedirectUri = RedirectUri,
            PostLogoutRedirectUri = RedirectUri,
            Scope = "openid profile email offline_access"
        }));
#endif

        builder.Services.AddSingleton<ISessionStore, SecureSessionStore>();

        return builder;
    }
}
