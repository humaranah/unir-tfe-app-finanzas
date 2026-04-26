using Auth0.OidcClient;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Core.ViewModels;

public partial class WelcomeViewModel(IAuth0Client client, IBrowserCookieCleaner cookieCleaner) : ObservableObject
{
    private readonly IAuth0Client _client = client;
    private readonly IBrowserCookieCleaner _cookieCleaner = cookieCleaner;

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
        Name = loginResult.User?.FindFirst(c => c.Type == "name")?.Value ?? string.Empty;
        Email = loginResult.User?.FindFirst(c => c.Type == "email")?.Value ?? string.Empty;
        IsAuthenticated = true;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        Error = string.Empty;
        _cookieCleaner.ClearCookies();
        await _client.LogoutAsync();
        Name = string.Empty;
        Email = string.Empty;
        IsAuthenticated = false;
    }
}
