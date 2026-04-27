using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Results;
using HA.TFG.AppFinanzas.App.Core.ViewModels;
using HA.TFG.AppFinanzas.Core.Authentication;
using NSubstitute;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class WelcomeViewModelTests
{
    private readonly IAuth0Client _auth0Client = Substitute.For<IAuth0Client>();
    private readonly ISessionStore _sessionStore = Substitute.For<ISessionStore>();

    private WelcomeViewModel CreateSut() =>
        new(_auth0Client, _sessionStore);

    private static string BuildIdToken(string name, string email)
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"alg":"none"}""")).TrimEnd('=');
        var payloadJson = JsonSerializer.Serialize(new { name, email, sub = "auth0|test" });
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson)).TrimEnd('=');
        return $"{header}.{payload}.";
    }

    private static LoginResult BuildLoginResult(ClaimsPrincipal? user = null, string? refreshToken = null)
    {
        var result = new LoginResult();
        SetProperty(result, "User", user ?? new ClaimsPrincipal(new ClaimsIdentity()));
        SetProperty(result, "RefreshToken", refreshToken);
        return result;
    }

    private static LoginResult BuildLoginError(string error)
    {
        var result = new LoginResult();
        SetProperty(result, "Error", error, searchBaseTypes: true);
        return result;
    }

    private static RefreshTokenResult BuildRefreshTokenResult(string? identityToken = null, string? refreshToken = null)
    {
        var result = new RefreshTokenResult();
        SetProperty(result, "IdentityToken", identityToken);
        SetProperty(result, "RefreshToken", refreshToken);
        return result;
    }

    private static RefreshTokenResult BuildRefreshTokenError(string error)
    {
        var result = new RefreshTokenResult();
        // Error is defined on the base class Result
        SetProperty(result, "Error", error, searchBaseTypes: true);
        return result;
    }

    private static void SetProperty(object obj, string propertyName, object? value, bool searchBaseTypes = true)
    {
        var type = obj.GetType();
        while (type is not null)
        {
            var prop = type.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (prop is not null)
            {
                prop.SetValue(obj, value);
                return;
            }

            if (!searchBaseTypes)
                break;

            type = type.BaseType;
        }
    }

    // ---------------------------------------------------------------------------
    // LoginAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task LoginAsync_WhenSuccessful_SetsUserAndAuthenticated()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("name", "Hugo"),
            new Claim("email", "hugo@test.com")
        ]));
        _auth0Client.LoginAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(BuildLoginResult(user, "rt_abc"));

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.True(sut.IsAuthenticated);
        Assert.Equal("Hugo", sut.Name);
        Assert.Equal("hugo@test.com", sut.Email);
        Assert.Empty(sut.Error);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessful_SavesRefreshToken()
    {
        _auth0Client.LoginAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(BuildLoginResult(refreshToken: "rt_abc"));

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        await _sessionStore.Received(1).SaveRefreshTokenAsync("rt_abc");
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessful_WithNoRefreshToken_DoesNotSaveToken()
    {
        _auth0Client.LoginAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(BuildLoginResult());

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        await _sessionStore.DidNotReceive().SaveRefreshTokenAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task LoginAsync_WhenUserCancels_DoesNotChangeState()
    {
        _auth0Client.LoginAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(BuildLoginError("UserCancel"));

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.False(sut.IsAuthenticated);
        Assert.Empty(sut.Error);
    }

    [Fact]
    public async Task LoginAsync_WhenError_SetsErrorMessage()
    {
        _auth0Client.LoginAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(BuildLoginError("access_denied"));

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.False(sut.IsAuthenticated);
        Assert.Contains("access_denied", sut.Error);
    }

    // ---------------------------------------------------------------------------
    // LogoutAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task LogoutAsync_ClearsUserState()
    {
        var sut = CreateSut();
        sut.IsAuthenticated = true;
        sut.Name = "Hugo";
        sut.Email = "hugo@test.com";

        await sut.LogoutCommand.ExecuteAsync(null);

        Assert.False(sut.IsAuthenticated);
        Assert.Empty(sut.Name);
        Assert.Empty(sut.Email);
    }

    [Fact]
    public async Task LogoutAsync_ClearsSessionStore()
    {
        var sut = CreateSut();
        await sut.LogoutCommand.ExecuteAsync(null);

        await _sessionStore.Received(1).ClearAsync();
    }

    [Fact]
    public async Task LogoutAsync_CallsAuth0Logout()
    {
        var sut = CreateSut();
        await sut.LogoutCommand.ExecuteAsync(null);

        await _auth0Client.Received(1).LogoutAsync(Arg.Any<bool>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    // ---------------------------------------------------------------------------
    // TryRestoreSessionAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task TryRestoreSessionAsync_WhenNoTokenStored_RemainsUnauthenticated()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns((string?)null);

        var sut = CreateSut();
        await sut.TryRestoreSessionAsync();

        Assert.False(sut.IsAuthenticated);
        await _auth0Client.DidNotReceive()
            .RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WhenRefreshFails_ClearsSessionAndRemainsUnauthenticated()
    {
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshTokenError("invalid_grant"));

        var sut = CreateSut();
        await sut.TryRestoreSessionAsync();

        Assert.False(sut.IsAuthenticated);
        await _sessionStore.Received(1).ClearAsync();
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WhenRefreshSucceeds_RestoresSession()
    {
        var idToken = BuildIdToken("Hugo", "hugo@test.com");
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshTokenResult(idToken, "rt_new"));

        var sut = CreateSut();
        await sut.TryRestoreSessionAsync();

        Assert.True(sut.IsAuthenticated);
        Assert.Equal("Hugo", sut.Name);
        Assert.Equal("hugo@test.com", sut.Email);
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WhenRefreshReturnsNewToken_SavesIt()
    {
        var idToken = BuildIdToken("Hugo", "hugo@test.com");
        _sessionStore.LoadRefreshTokenAsync().Returns("rt_old");
        _auth0Client.RefreshTokenAsync("rt_old", Arg.Any<CancellationToken>())
            .Returns(BuildRefreshTokenResult(idToken, "rt_new"));

        var sut = CreateSut();
        await sut.TryRestoreSessionAsync();

        await _sessionStore.Received(1).SaveRefreshTokenAsync("rt_new");
    }

    // ---------------------------------------------------------------------------
    // IsNotAuthenticated (propiedad derivada)
    // ---------------------------------------------------------------------------

    [Fact]
    public void IsNotAuthenticated_IsInverseOfIsAuthenticated()
    {
        var sut = CreateSut();

        Assert.True(sut.IsNotAuthenticated);
        sut.IsAuthenticated = true;
        Assert.False(sut.IsNotAuthenticated);
    }
}
