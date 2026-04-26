namespace HA.TFG.AppFinanzas.App.Authentication;

public partial class WebViewPage : ContentPage
{
    private readonly string _callbackUri;
    private readonly TaskCompletionSource<string?> _tcs;

    public WebViewPage(string startUri, string callbackUri, TaskCompletionSource<string?> tcs)
    {
        InitializeComponent();
        _callbackUri = callbackUri;
        _tcs = tcs;
        AuthWebView.Source = startUri;
    }

    private void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith(_callbackUri, StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            _tcs.TrySetResult(e.Url);
            MainThread.BeginInvokeOnMainThread(async () =>
                await Navigation.PopModalAsync());
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        MainThread.BeginInvokeOnMainThread(async () =>
            await Navigation.PopModalAsync());
    }
}
