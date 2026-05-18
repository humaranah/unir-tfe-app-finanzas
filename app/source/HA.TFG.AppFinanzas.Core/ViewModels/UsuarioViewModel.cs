using Auth0.OidcClient;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Authentication;
using System.Text;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class UsuarioViewModel(
    IAuth0Client auth0Client,
    ISessionStore sessionStore,
    IUsuarioEnsureService usuarioEnsureService,
    IBackendHealthService backendHealthService) : ObservableObject
{
    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string Error { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    public partial bool IsAuthenticated { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    public bool IsNotBusy => !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsNotAuthenticated => !IsAuthenticated;

    public async Task TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await sessionStore.LoadRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var result = await auth0Client.RefreshTokenAsync(refreshToken, cancellationToken);
        if (result.IsError)
        {
            await sessionStore.ClearAsync();
            return;
        }

        if (!string.IsNullOrEmpty(result.RefreshToken))
            await sessionStore.SaveRefreshTokenAsync(result.RefreshToken);
        if (!string.IsNullOrEmpty(result.AccessToken))
            await sessionStore.SaveAccessTokenAsync(result.AccessToken);

        ParseIdentityToken(result.IdentityToken);
        IsAuthenticated = true;

        await usuarioEnsureService.EnsureUsuarioAsync(cancellationToken);
    }

    [RelayCommand]
    public async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        Error = string.Empty;
        IsBusy = true;

        try
        {
            var backendAvailable = await backendHealthService.IsAvailableAsync(cancellationToken);
            if (!backendAvailable)
            {
                Error = "El servidor no está disponible. Inténtalo de nuevo más tarde.";
                return;
            }

            var result = await auth0Client.LoginAsync(cancellationToken: cancellationToken);
            if (result.IsError)
            {
                if (result.Error != "UserCancel")
                    Error = result.ErrorDescription ?? result.Error ?? "Error al iniciar sesión.";
                return;
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
                await sessionStore.SaveRefreshTokenAsync(result.RefreshToken);
            if (!string.IsNullOrEmpty(result.AccessToken))
                await sessionStore.SaveAccessTokenAsync(result.AccessToken);

            Name = result.User?.FindFirst("name")?.Value
                ?? result.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
                ?? string.Empty;
            Email = result.User?.FindFirst("email")?.Value
                ?? result.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                ?? string.Empty;

            IsAuthenticated = true;

            await usuarioEnsureService.EnsureUsuarioAsync(cancellationToken);

            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await auth0Client.LogoutAsync(cancellationToken: cancellationToken);
        await sessionStore.ClearAsync();
        IsAuthenticated = false;
        Name = string.Empty;
        Email = string.Empty;
    }

    private void ParseIdentityToken(string? idToken)
    {
        if (string.IsNullOrEmpty(idToken))
            return;

        try
        {
            var parts = idToken.Split('.');
            if (parts.Length < 2)
                return;

            var payload = parts[1];
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Name = root.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
            Email = root.TryGetProperty("email", out var e) ? e.GetString() ?? string.Empty : string.Empty;
        }
        catch
        {
            // Si el token no se puede parsear, dejamos Name/Email vacíos
        }
    }
}
