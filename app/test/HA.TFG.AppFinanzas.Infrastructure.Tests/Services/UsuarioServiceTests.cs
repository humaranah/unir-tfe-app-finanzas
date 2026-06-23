using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient.Results;
using HA.TFG.AppFinanzas.Core.Authentication;
using HA.TFG.AppFinanzas.Infrastructure.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.Infrastructure.Tests.Services;

public class UsuarioServiceTests
{
    private readonly IAuth0Client _auth0Client = Substitute.For<IAuth0Client>();
    private readonly ISessionStore _sessionStore = Substitute.For<ISessionStore>();
    private readonly IUsuarioEnsureService _ensureService = Substitute.For<IUsuarioEnsureService>();
    private readonly IBackendHealthService _healthService = Substitute.For<IBackendHealthService>();

    private UsuarioService CreateSut() =>
        new(_auth0Client, _sessionStore, _ensureService, _healthService);

    private static string BuildIdToken(string nombre = "Hugo", string email = "hugo@test.com")
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"alg":"none"}""")).TrimEnd('=');
        var payloadJson = JsonSerializer.Serialize(new { name = nombre, email, sub = "auth0|test" });
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson)).TrimEnd('=');
        return $"{header}.{payload}.";
    }

    private static RefreshTokenResult BuildRefreshResult(
        string? identityToken = null, string? refreshToken = null, string? accessToken = null)
    {
        var r = new RefreshTokenResult();
        SetProp(r, "IdentityToken", identityToken);
        SetProp(r, "RefreshToken", refreshToken);
        SetProp(r, "AccessToken", accessToken);
        return r;
    }

    private static RefreshTokenResult BuildRefreshError(string error)
    {
        var r = new RefreshTokenResult();
        SetProp(r, "Error", error, searchBase: true);
        return r;
    }

    private static void SetProp(object obj, string name, object? value, bool searchBase = true)
    {
        var type = obj.GetType();
        while (type is not null)
        {
            var p = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (p is not null) { p.SetValue(obj, value); return; }
            if (!searchBase) break;
            type = type.BaseType;
        }
    }

    #region TryRestoreSessionAsync — sin token

    [Fact]
    public async Task TryRestoreSessionAsync_WhenNoToken_ReturnsNull()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns((string?)null);

        var result = await CreateSut().TryRestoreSessionAsync();

        Assert.Null(result);
        await _auth0Client.DidNotReceive().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region TryRestoreSessionAsync — refresh falla

    [Fact]
    public async Task TryRestoreSessionAsync_WhenRefreshFails_ClearsSessionAndReturnsNull()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshError("invalid_grant"));

        var result = await CreateSut().TryRestoreSessionAsync();

        Assert.Null(result);
        await _sessionStore.Received(1).ClearAsync();
    }

    #endregion

    #region TryRestoreSessionAsync — backend no disponible

    [Fact]
    public async Task TryRestoreSessionAsync_WhenBackendUnavailable_ReturnsInfoWithoutCallingEnsure()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshResult(BuildIdToken(), "rt_new"));
        _healthService.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var result = await CreateSut().TryRestoreSessionAsync();

        Assert.NotNull(result);
        Assert.Equal("Hugo", result.Nombre);
        await _ensureService.DidNotReceive().EnsureUsuarioAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WhenBackendUnavailable_DoesNotClearSession()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshResult(BuildIdToken(), "rt_new"));
        _healthService.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        await CreateSut().TryRestoreSessionAsync();

        await _sessionStore.DidNotReceive().ClearAsync();
    }

    #endregion

    #region TryRestoreSessionAsync — backend disponible, Ensure falla

    [Fact]
    public async Task TryRestoreSessionAsync_WhenEnsureFails_ClearsSessionAndReturnsNull()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshResult(BuildIdToken(), "rt_new"));
        _healthService.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _ensureService.EnsureUsuarioAsync(Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Error del servidor"));

        var result = await CreateSut().TryRestoreSessionAsync();

        Assert.Null(result);
        await _sessionStore.Received(1).ClearAsync();
    }

    #endregion

    #region TryRestoreSessionAsync — éxito completo

    [Fact]
    public async Task TryRestoreSessionAsync_WhenSuccessful_ReturnsUserInfo()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshResult(BuildIdToken("Ana", "ana@test.com"), "rt_new", "at_new"));
        _healthService.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateSut().TryRestoreSessionAsync();

        Assert.NotNull(result);
        Assert.Equal("Ana", result.Nombre);
        Assert.Equal("ana@test.com", result.Email);
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WhenSuccessful_SavesNewTokens()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshResult(BuildIdToken(), "rt_new", "at_new"));
        _healthService.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);

        await CreateSut().TryRestoreSessionAsync();

        await _sessionStore.Received(1).SaveRefreshTokenAsync("rt_new");
        await _sessionStore.Received(1).SaveAccessTokenAsync("at_new");
    }

    #endregion
}
