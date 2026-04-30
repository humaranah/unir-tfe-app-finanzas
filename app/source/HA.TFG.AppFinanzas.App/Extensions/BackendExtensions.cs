using HA.TFG.AppFinanzas.App.Authentication;
using HA.TFG.AppFinanzas.App.Http;
using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Extensions;

internal static class BackendExtensions
{
    internal static MauiAppBuilder AddBackend(this MauiAppBuilder builder)
    {
        var baseUrl = builder.Configuration["Backend:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Backend:BaseUrl is not configured.");

        builder.Services.AddSingleton<ITokenProvider, AuthTokenProvider>();
        builder.Services.AddSingleton<IUsuarioSyncService, UsuariosApiClient>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        builder.Services.AddHttpClient("Backend", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

        return builder;
    }
}
