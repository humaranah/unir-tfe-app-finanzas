using HA.TFG.AppFinanzas.App.Extensions;
using HA.TFG.AppFinanzas.App.Navigation;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<Button, Microsoft.Maui.Handlers.ButtonHandler>();
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentIcons");
            });

        var assembly = typeof(MauiProgram).Assembly;

        using var appSettingsStream = assembly.GetManifestResourceStream("HA.TFG.AppFinanzas.App.appsettings.json");
        if (appSettingsStream is not null)
            builder.Configuration.AddJsonStream(appSettingsStream);

        using var secretsStream = assembly.GetManifestResourceStream("HA.TFG.AppFinanzas.App.appsettings.secrets.json");
        if (secretsStream is not null)
            builder.Configuration.AddJsonStream(secretsStream);

        builder.AddAuth0();
        builder.AddBackend();

        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<UsuarioViewModel>();
        builder.Services.AddSingleton<CrearCuentaViewModel>();
        builder.Services.AddSingleton<MovimientosViewModel>();
        builder.Services.AddTransient<MovimientoViewModel>();
        builder.Services.AddSingleton<RecomendacionesViewModel>();
        builder.Services.AddSingleton<Views.LoginPage>();
        builder.Services.AddSingleton<Views.SplashLoadingPage>();
        builder.Services.AddTransient<Views.Pages.CrearCuentaPage>();
        builder.Services.AddSingleton<Views.Pages.MovimientosPage>();
        builder.Services.AddTransient<Views.Pages.CrearMovimientoPage>();
        builder.Services.AddSingleton<Views.Pages.RecomendacionesPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
