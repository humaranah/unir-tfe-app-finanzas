using Auth0.OidcClient;

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

        builder.Services.AddSingleton<IAuth0Client>(new Auth0Client(new Auth0ClientOptions
        {
            Domain = domain,
            ClientId = clientId,
            RedirectUri = RedirectUri,
            PostLogoutRedirectUri = RedirectUri
        }));

        return builder;
    }
}
