using HA.TFG.AppFinanzas.App.Core.ViewModels;
using HA.TFG.AppFinanzas.App.Extensions;
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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var assembly = typeof(MauiProgram).Assembly;

        using var appSettingsStream = assembly.GetManifestResourceStream("HA.TFG.AppFinanzas.App.appsettings.json");
        if (appSettingsStream is not null)
            builder.Configuration.AddJsonStream(appSettingsStream);

        using var secretsStream = assembly.GetManifestResourceStream("HA.TFG.AppFinanzas.App.appsettings.secrets.json");
        if (secretsStream is not null)
            builder.Configuration.AddJsonStream(secretsStream);

        builder.AddAuth0();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<WelcomeViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
