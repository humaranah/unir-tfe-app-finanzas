using Auth0.OidcClient;
using HA.TFG.AppFinanzas.Core.Authentication;
using HA.TFG.AppFinanzas.Core.Models;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.Infrastructure.Services;

public class UsuarioService(
    IAuth0Client auth0Client,
    ISessionStore sessionStore,
    IUsuarioEnsureService usuarioEnsureService,
    IBackendHealthService backendHealthService) : IUsuarioService
{
    public async Task<UsuarioInfo?> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await sessionStore.LoadRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var result = await auth0Client.RefreshTokenAsync(refreshToken, cancellationToken);
        if (result.IsError)
        {
            await sessionStore.ClearAsync();
            return null;
        }

        if (!string.IsNullOrEmpty(result.RefreshToken))
            await sessionStore.SaveRefreshTokenAsync(result.RefreshToken);
        if (!string.IsNullOrEmpty(result.AccessToken))
            await sessionStore.SaveAccessTokenAsync(result.AccessToken);

        var info = ParseIdentityToken(result.IdentityToken);

        try
        {
            var backendAvailable = await backendHealthService.IsAvailableAsync(cancellationToken);
            if (!backendAvailable)
                return info;

            await usuarioEnsureService.EnsureUsuarioAsync(cancellationToken);
        }
        catch
        {
            await sessionStore.ClearAsync();
            return null;
        }

        return info;
    }

    public async Task<UsuarioInfo?> LoginAsync(CancellationToken cancellationToken = default)
    {
        var backendAvailable = await backendHealthService.IsAvailableAsync(cancellationToken);
        if (!backendAvailable)
            throw new InvalidOperationException("El servidor no está disponible. Inténtalo de nuevo más tarde.");

        var result = await auth0Client.LoginAsync(cancellationToken: cancellationToken);

        if (result.IsError)
        {
            if (result.Error == "UserCancel")
                return null;

            throw new InvalidOperationException(result.ErrorDescription ?? result.Error ?? "Error al iniciar sesión.");
        }

        if (!string.IsNullOrEmpty(result.RefreshToken))
            await sessionStore.SaveRefreshTokenAsync(result.RefreshToken);
        if (!string.IsNullOrEmpty(result.AccessToken))
            await sessionStore.SaveAccessTokenAsync(result.AccessToken);

        var nombre = result.User?.FindFirst("name")?.Value
            ?? result.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
            ?? string.Empty;
        var email = result.User?.FindFirst("email")?.Value
            ?? result.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
            ?? string.Empty;

        try
        {
            await usuarioEnsureService.EnsureUsuarioAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await sessionStore.ClearAsync();
            throw new InvalidOperationException("No se pudo sincronizar el usuario con el servidor.", ex);
        }

        return new UsuarioInfo(email, nombre, null, null, false, null);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await auth0Client.LogoutAsync(cancellationToken: cancellationToken);
        }
        finally
        {
            await sessionStore.ClearAsync();
        }
    }

    private static UsuarioInfo? ParseIdentityToken(string? idToken)
    {
        if (string.IsNullOrEmpty(idToken))
            return null;

        try
        {
            var parts = idToken.Split('.');
            if (parts.Length < 2)
                return null;

            var payload = parts[1];
            var json = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(payload));

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var nombre = root.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
            var email = root.TryGetProperty("email", out var e) ? e.GetString() ?? string.Empty : string.Empty;

            return new UsuarioInfo(email, nombre, null, null, false, null);
        }
        catch
        {
            return null;
        }
    }
}
