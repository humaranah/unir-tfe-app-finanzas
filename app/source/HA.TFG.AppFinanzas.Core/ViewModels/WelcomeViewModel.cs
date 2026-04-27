using Auth0.OidcClient;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Core.ViewModels;

public partial class WelcomeViewModel(
    IAuth0Client client,
    IBrowserCookieCleaner cookieCleaner,
    ISessionStore sessionStore) : ObservableObject
{
    private readonly IAuth0Client _client = client;
    private readonly IBrowserCookieCleaner _cookieCleaner = cookieCleaner;
    private readonly ISessionStore _sessionStore = sessionStore;

    public string WelcomeTitle { get; } = "Hello, World!";

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _error;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    private bool _isAuthenticated;

    public bool IsNotAuthenticated => !IsAuthenticated;

    public async Task TryRestoreSessionAsync()
    {
        var refreshToken = await _sessionStore.LoadRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var result = await _client.RefreshTokenAsync(refreshToken, cancellationToken: default);
        if (result.IsError)
        {
            await _sessionStore.ClearAsync();
            return;
        }

        if (!string.IsNullOrEmpty(result.RefreshToken))
            await _sessionStore.SaveRefreshTokenAsync(result.RefreshToken);

        var claims = ParseIdToken(result.IdentityToken);
        Name = claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty;
        Email = claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty;
        IsAuthenticated = true;
    }

    private static IEnumerable<System.Security.Claims.Claim> ParseIdToken(string? idToken)
    {
        if (string.IsNullOrEmpty(idToken))
            return [];

        var parts = idToken.Split('.');
        if (parts.Length < 2)
            return [];

        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = System.Text.Json.JsonDocument.Parse(json);

        return doc.RootElement.EnumerateObject()
            .Select(p => new System.Security.Claims.Claim(p.Name, p.Value.ToString()));
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        Error = string.Empty;
        var loginResult = await _client.LoginAsync();
        if (loginResult.IsError)
        {
            if (loginResult.Error == "UserCancel")
                return;
            Error = $"Login failed: {loginResult.Error}";
            return;
        }

        if (!string.IsNullOrEmpty(loginResult.RefreshToken))
            await _sessionStore.SaveRefreshTokenAsync(loginResult.RefreshToken);

        Name = loginResult.User?.FindFirst(c => c.Type == "name")?.Value ?? string.Empty;
        Email = loginResult.User?.FindFirst(c => c.Type == "email")?.Value ?? string.Empty;
        IsAuthenticated = true;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        Error = string.Empty;
        _cookieCleaner.ClearCookies();
        await _sessionStore.ClearAsync();
        await _client.LogoutAsync();
        Name = string.Empty;
        Email = string.Empty;
        IsAuthenticated = false;
    }
}
