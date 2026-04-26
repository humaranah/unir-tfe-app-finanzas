using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Authentication;

internal sealed class NullBrowserCookieCleaner : IBrowserCookieCleaner
{
    public void ClearCookies() { }
}
