using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient.Results;
using HA.TFG.AppFinanzas.Core.Authentication;
using HA.TFG.AppFinanzas.Infrastructure.Authentication;
using NSubstitute;

namespace HA.TFG.AppFinanzas.Infrastructure.Tests.Authentication;

public class AuthTokenProviderTests
{
    #region Setup

    private readonly IAuth0Client _mockAuth0Client = Substitute.For<IAuth0Client>();
    private readonly ISessionStore _mockSessionStore = Substitute.For<ISessionStore>();
    private readonly AuthTokenProvider _provider;

    public AuthTokenProviderTests()
    {
        _provider = new AuthTokenProvider(_mockAuth0Client, _mockSessionStore);
    }

    #endregion

    #region GetAccessTokenAsync - Happy Path

    [Fact]
    public async Task GetAccessTokenAsync_WithValidAccessToken_ReturnsToken()
    {
        // Arrange
        var expectedToken = "valid_access_token";
        _mockSessionStore.LoadAccessTokenAsync()
            .Returns(expectedToken);

        // Act
        var result = await _provider.GetAccessTokenAsync();

        // Assert
        Assert.Equal(expectedToken, result);
        await _mockSessionStore.Received(1).LoadAccessTokenAsync();
    }

    #endregion

    #region GetAccessTokenAsync - Token Refresh

    [Fact]
    public async Task GetAccessTokenAsync_WithNoAccessToken_RefreshesUsingRefreshToken()
    {
        // Arrange
        var refreshToken = "valid_refresh_token";
        var newAccessToken = "new_access_token";
        var newRefreshToken = "new_refresh_token";

        _mockSessionStore.LoadAccessTokenAsync().Returns((string?)null);
        _mockSessionStore.LoadRefreshTokenAsync().Returns(refreshToken);

        var refreshResult = Substitute.For<RefreshTokenResult>();
        refreshResult.IsError.Returns(false);
        refreshResult.AccessToken.Returns(newAccessToken);
        refreshResult.RefreshToken.Returns(newRefreshToken);

        _mockAuth0Client.RefreshTokenAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(refreshResult);

        // Act
        var result = await _provider.GetAccessTokenAsync();

        // Assert
        Assert.Equal(newAccessToken, result);
        await _mockSessionStore.Received(1).SaveAccessTokenAsync(newAccessToken);
        await _mockSessionStore.Received(1).SaveRefreshTokenAsync(newRefreshToken);
    }

    [Fact]
    public async Task GetAccessTokenAsync_WithRefreshError_ReturnsNull()
    {
        // Arrange
        var refreshToken = "invalid_refresh_token";

        _mockSessionStore.LoadAccessTokenAsync().Returns((string?)null);
        _mockSessionStore.LoadRefreshTokenAsync().Returns(refreshToken);

        var refreshResult = Substitute.For<RefreshTokenResult>();
        refreshResult.IsError.Returns(true);

        _mockAuth0Client.RefreshTokenAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(refreshResult);

        // Act
        var result = await _provider.GetAccessTokenAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAccessTokenAsync - No Token Available

    [Fact]
    public async Task GetAccessTokenAsync_WithNoTokens_ReturnsNull()
    {
        // Arrange
        _mockSessionStore.LoadAccessTokenAsync().Returns((string?)null);
        _mockSessionStore.LoadRefreshTokenAsync().Returns((string?)null);

        // Act
        var result = await _provider.GetAccessTokenAsync();

        // Assert
        Assert.Null(result);
        await _mockAuth0Client.DidNotReceive().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccessTokenAsync_WithEmptyAccessToken_AttemptsRefresh()
    {
        // Arrange
        var refreshToken = "valid_refresh_token";
        var newAccessToken = "new_access_token";

        _mockSessionStore.LoadAccessTokenAsync().Returns(string.Empty);
        _mockSessionStore.LoadRefreshTokenAsync().Returns(refreshToken);

        var refreshResult = Substitute.For<RefreshTokenResult>();
        refreshResult.IsError.Returns(false);
        refreshResult.AccessToken.Returns(newAccessToken);
        refreshResult.RefreshToken.Returns((string?)null);

        _mockAuth0Client.RefreshTokenAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(refreshResult);

        // Act
        var result = await _provider.GetAccessTokenAsync();

        // Assert
        Assert.Equal(newAccessToken, result);
    }

    #endregion

    #region Token Update Scenarios

    [Fact]
    public async Task GetAccessTokenAsync_WithPartialRefreshResponse_UpdatesOnlyAvailableTokens()
    {
        // Arrange - New access token but no refresh token in response
        var oldRefreshToken = "old_refresh_token";
        var newAccessToken = "new_access_token";

        _mockSessionStore.LoadAccessTokenAsync().Returns((string?)null);
        _mockSessionStore.LoadRefreshTokenAsync().Returns(oldRefreshToken);

        var refreshResult = Substitute.For<RefreshTokenResult>();
        refreshResult.IsError.Returns(false);
        refreshResult.AccessToken.Returns(newAccessToken);
        refreshResult.RefreshToken.Returns((string?)null);

        _mockAuth0Client.RefreshTokenAsync(oldRefreshToken, Arg.Any<CancellationToken>())
            .Returns(refreshResult);

        // Act
        var result = await _provider.GetAccessTokenAsync();

        // Assert
        Assert.Equal(newAccessToken, result);
        await _mockSessionStore.Received(1).SaveAccessTokenAsync(newAccessToken);
        await _mockSessionStore.DidNotReceive().SaveRefreshTokenAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAccessTokenAsync_PreservesExistingTokenIfRefreshOnlyReturnsPartial()
    {
        // Arrange
        var refreshToken = "valid_refresh_token";

        _mockSessionStore.LoadAccessTokenAsync().Returns((string?)null);
        _mockSessionStore.LoadRefreshTokenAsync().Returns(refreshToken);

        var refreshResult = Substitute.For<RefreshTokenResult>();
        refreshResult.IsError.Returns(false);
        refreshResult.AccessToken.Returns((string?)null);  // No access token in response
        refreshResult.RefreshToken.Returns((string?)null);

        _mockAuth0Client.RefreshTokenAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(refreshResult);

        // Act
        var result = await _provider.GetAccessTokenAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Concurrency and Edge Cases

    [Fact]
    public async Task GetAccessTokenAsync_WhenLoadAccessTokenThrows_PropagatesException()
    {
        // Arrange
        var exception = new InvalidOperationException("Session store failed");
        _mockSessionStore.LoadAccessTokenAsync()
            .Returns(Task.FromException<string?>(exception));

        // Act & Assert
        var result = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.GetAccessTokenAsync());

        Assert.Equal("Session store failed", result.Message);
    }

    #endregion
}
