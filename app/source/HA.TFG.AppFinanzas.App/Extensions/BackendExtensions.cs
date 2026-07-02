using HA.TFG.AppFinanzas.App.Services;
using HA.TFG.AppFinanzas.Core.Features.Authentication;
using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using HA.TFG.AppFinanzas.Core.Features.Movimientos;
using HA.TFG.AppFinanzas.Core.Features.Recomendaciones;
using HA.TFG.AppFinanzas.Infrastructure;
using HA.TFG.AppFinanzas.Infrastructure.Authentication;
using HA.TFG.AppFinanzas.Infrastructure.Clients;
using HA.TFG.AppFinanzas.Infrastructure.Http;
using HA.TFG.AppFinanzas.Infrastructure.Services;

namespace HA.TFG.AppFinanzas.App.Extensions;

internal static class BackendExtensions
{
    internal static MauiAppBuilder AddBackend(this MauiAppBuilder builder)
    {
        var baseUrl = builder.Configuration["Backend:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Backend:BaseUrl is not configured.");

        builder.Services.AddSingleton<ITokenProvider, AuthTokenProvider>();
        builder.Services.AddSingleton<IUsuarioEnsureService, UsuariosApiClient>();
        builder.Services.AddSingleton<IBackendHealthService, BackendHealthClient>();
        builder.Services.AddSingleton<IUsuarioService, UsuarioService>();
        builder.Services.AddSingleton<ICuentasService, CuentasApiClient>();
        builder.Services.AddSingleton<IMovimientosService, MovimientosApiClient>();
        builder.Services.AddSingleton<IRecomendacionesService, RecomendacionesApiClient>();
        builder.Services.AddSingleton<IComprobantePickerService, ComprobantePickerService>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        builder.Services.AddHttpClient(HttpClientNames.Backend, client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

        // Cliente sin autenticación, solo para /health
        builder.Services.AddHttpClient(HttpClientNames.BackendHealth, client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return builder;
    }
}
