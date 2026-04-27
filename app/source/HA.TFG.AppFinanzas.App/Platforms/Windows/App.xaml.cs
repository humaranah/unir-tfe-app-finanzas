using Auth0Activator = Auth0.OidcClient.Platforms.Windows.Activator;

namespace HA.TFG.AppFinanzas.App.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        if (Auth0Activator.Default.CheckRedirectionActivation())
        {
            // La aplicación fue activada mediante una redirección desde el navegador del sistema.
            // El OidcClient gestionará la redirección y completará el flujo de inicio de sesión.
            return;
        }

        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
