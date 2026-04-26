using Duende.IdentityModel.OidcClient.Browser;
using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Authentication;

internal sealed class EmbeddedBrowser : Duende.IdentityModel.OidcClient.Browser.IBrowser, IBrowserCookieCleaner
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<string?>();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = new WebViewPage(options.StartUrl, options.EndUrl, tcs);
            var currentPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (currentPage is not null)
                await currentPage.Navigation.PushModalAsync(page);
        });

        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());

        string? resultUrl;
        try
        {
            resultUrl = await tcs.Task;
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };
        }

        if (resultUrl is null)
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };

        return new BrowserResult { ResultType = BrowserResultType.Success, Response = resultUrl };
    }

    void IBrowserCookieCleaner.ClearCookies()
    {
        // En Windows con WebView2, las cookies se gestionan por perfil de usuario.
        // Limpiar el almacenamiento en memoria es suficiente; el logout de Auth0
        // invalida el token en el servidor.
    }
}
