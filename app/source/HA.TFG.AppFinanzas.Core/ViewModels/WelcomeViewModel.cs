using Auth0.OidcClient;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.App.Core.ViewModels;

public partial class WelcomeViewModel(
    IAuth0Client client,
    ISessionStore sessionStore,
    IUsuarioSyncService usuarioSyncService) : ObservableObject
{
    private readonly IAuth0Client _client = client;
    private readonly ISessionStore _sessionStore = sessionStore;
    private readonly IUsuarioSyncService _usuarioSyncService = usuarioSyncService;

    public string WelcomeTitle { get; } = "Hello, World!";

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string Error { get; set; } = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(Error);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    public partial bool IsAuthenticated { get; set; } = false;

    public bool IsNotAuthenticated => !IsAuthenticated;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; } = false;

    public bool IsNotBusy => !IsBusy;

    public event EventHandler? LoginSucceeded;

    public async Task TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await _sessionStore.LoadRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var result = await _client.RefreshTokenAsync(refreshToken, cancellationToken);
        if (result.IsError)
        {
            await _sessionStore.ClearAsync();
            return;
        }

        if (!string.IsNullOrEmpty(result.RefreshToken))
            await _sessionStore.SaveRefreshTokenAsync(result.RefreshToken);

        if (!string.IsNullOrEmpty(result.AccessToken))
            await _sessionStore.SaveAccessTokenAsync(result.AccessToken);

        try
        {
            var claims = ParseIdToken(result.IdentityToken);
            var usuarioInfo = BuildUsuarioInfo(claims);

            try
            {
                await _usuarioSyncService.EnsureUsuarioAsync(usuarioInfo, cancellationToken);
            }
            catch
            {
                await _sessionStore.ClearAsync();
                return;
            }

            Name = usuarioInfo.Nombre;
            Email = usuarioInfo.Email;
            IsAuthenticated = true;
        }
        catch
        {
            await _sessionStore.ClearAsync();
        }
    }

    private static Claim[] ParseIdToken(string? idToken)
    {
        if (string.IsNullOrEmpty(idToken))
            return [];

        var parts = idToken.Split('.');
        if (parts.Length < 2)
            return [];

        var payload = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));

        using var doc = JsonDocument.Parse(json);
        return [.. doc.RootElement
            .EnumerateObject()
            .Select(p => new Claim(p.Name, p.Value.ToString()))];
    }

    private static UsuarioInfo BuildUsuarioInfo(IEnumerable<Claim> claims)
    {
        string? Get(string type) => claims.FirstOrDefault(c => c.Type == type)?.Value;

        var updatedAtRaw = Get("updated_at");
        DateTimeOffset? updatedAt = updatedAtRaw is not null && DateTimeOffset.TryParse(updatedAtRaw, out var parsed)
            ? parsed
            : null;

        return new UsuarioInfo(
            Email: Get("email") ?? string.Empty,
            Nombre: Get("name") ?? string.Empty,
            FotoPerfil: Get("picture"),
            Proveedor: Get("sub")?.Split('|')[0],
            EmailVerificado: string.Equals(Get("email_verified"), "true", StringComparison.OrdinalIgnoreCase),
            UltimaActualizacion: updatedAt);
    }

    [RelayCommand]
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        Error = string.Empty;
        IsBusy = true;
        try
        {
            var loginResult = await _client.LoginAsync();
            if (loginResult.IsError)
            {
                if (loginResult.Error != "UserCancel")
                    Error = $"Error al iniciar sesión: {loginResult.Error}";
                return;
            }

            if (!string.IsNullOrEmpty(loginResult.RefreshToken))
                await _sessionStore.SaveRefreshTokenAsync(loginResult.RefreshToken);

            if (!string.IsNullOrEmpty(loginResult.AccessToken))
                await _sessionStore.SaveAccessTokenAsync(loginResult.AccessToken);

            var usuarioInfo = BuildUsuarioInfo(loginResult.User?.Claims ?? []);

            try
            {
                await _usuarioSyncService.EnsureUsuarioAsync(usuarioInfo, cancellationToken);
            }
            catch
            {
                await _sessionStore.ClearAsync();
                Error = "No se pudo sincronizar tu usuario con el backend.";
                return;
            }

            Name = usuarioInfo.Nombre;
            Email = usuarioInfo.Email;
            IsAuthenticated = true;

            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        Error = string.Empty;
        await _sessionStore.ClearAsync();
        await _client.LogoutAsync();
        Name = string.Empty;
        Email = string.Empty;
        IsAuthenticated = false;
    }
}
