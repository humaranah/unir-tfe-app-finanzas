using HA.TFG.AppFinanzas.Core.Features.Authentication;
using HA.TFG.AppFinanzas.Core.Models;
using HA.TFG.AppFinanzas.Core.Tests.Fixtures;
using HA.TFG.AppFinanzas.Core.Features.Authentication;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.Core.Tests.ViewModels;

public class UsuarioViewModelTests
{
    private readonly IUsuarioService _usuarioService = Substitute.For<IUsuarioService>();

    private UsuarioViewModel CreateSut() => new(_usuarioService);

    private static UsuarioInfo BuildInfo(string nombre = "Hugo", string email = "hugo@test.com") =>
        new(email, nombre, null, null, false, null);

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_WhenSuccessful_SetsUserAndAuthenticated()
    {
        _usuarioService.LoginAsync(Arg.Any<CancellationToken>()).Returns(BuildInfo());

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.True(sut.IsAuthenticated),
            () => Assert.Equal("Hugo", sut.Name),
            () => Assert.Equal("hugo@test.com", sut.Email),
            () => Assert.Empty(sut.Error));
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessful_RaisesLoginSucceeded()
    {
        _usuarioService.LoginAsync(Arg.Any<CancellationToken>()).Returns(BuildInfo());
        var sut = CreateSut();
        var raised = false;
        sut.LoginSucceeded += (_, _) => raised = true;

        await sut.LoginCommand.ExecuteAsync(null);

        Assert.True(raised);
    }

    [Fact]
    public async Task LoginAsync_WhenUserCancels_DoesNotChangeState()
    {
        _usuarioService.LoginAsync(Arg.Any<CancellationToken>()).Returns((UsuarioInfo?)null);

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.False(sut.IsAuthenticated),
            () => Assert.Empty(sut.Error));
    }

    [Fact]
    public async Task LoginAsync_WhenServiceThrows_SetsError()
    {
        _usuarioService.LoginAsync(Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("El servidor no está disponible."));

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.False(sut.IsAuthenticated),
            () => Assert.Contains("disponible", sut.Error),
            () => Assert.False(sut.IsBusy));
    }

    [Fact]
    public async Task LoginAsync_ClearsPreviousError()
    {
        var sut = CreateSut();
        sut.Error = "Previous error message";
        _usuarioService.LoginAsync(Arg.Any<CancellationToken>())
            .Returns(TestDataBuilder.Usuario.Build());

        await sut.LoginCommand.ExecuteAsync(null);

        Assert.Empty(sut.Error);
    }

    [Fact]
    public async Task LoginAsync_StaysBusyFalseAfterCompletion()
    {
        _usuarioService.LoginAsync(Arg.Any<CancellationToken>())
            .Returns(TestDataBuilder.Usuario.Build());

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.False(sut.IsBusy);
    }

    #endregion

    #region LogoutAsync

    [Fact]
    public async Task LogoutAsync_ClearsUserState()
    {
        var sut = CreateSut();
        sut.IsAuthenticated = true;
        sut.Name = "Hugo";
        sut.Email = "hugo@test.com";

        await sut.LogoutCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.False(sut.IsAuthenticated),
            () => Assert.Empty(sut.Name),
            () => Assert.Empty(sut.Email));
    }

    [Fact]
    public async Task LogoutAsync_CallsService()
    {
        var sut = CreateSut();
        await sut.LogoutCommand.ExecuteAsync(null);

        await _usuarioService.Received(1).LogoutAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region TryRestoreSessionAsync

    [Fact]
    public async Task TryRestoreSessionAsync_WhenServiceReturnsNull_RemainsUnauthenticated()
    {
        _usuarioService.TryRestoreSessionAsync(Arg.Any<CancellationToken>()).Returns((UsuarioInfo?)null);

        var sut = CreateSut();
        await sut.TryRestoreSessionAsync();

        Assert.False(sut.IsAuthenticated);
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WhenServiceReturnsInfo_RestoresSession()
    {
        _usuarioService.TryRestoreSessionAsync(Arg.Any<CancellationToken>()).Returns(BuildInfo());

        var sut = CreateSut();
        await sut.TryRestoreSessionAsync();

        Assert.Multiple(
            () => Assert.True(sut.IsAuthenticated),
            () => Assert.Equal("Hugo", sut.Name),
            () => Assert.Equal("hugo@test.com", sut.Email));
    }

    [Fact]
    public async Task TryRestoreSessionAsync_MultipleAttempts_ConsistentBehavior()
    {
        _usuarioService.TryRestoreSessionAsync(Arg.Any<CancellationToken>())
            .Returns(TestDataBuilder.Usuario.Build());

        var sut = CreateSut();
        await sut.TryRestoreSessionAsync();
        var firstState = sut.IsAuthenticated;

        await sut.TryRestoreSessionAsync();
        var secondState = sut.IsAuthenticated;

        Assert.Equal(firstState, secondState);
    }

    #endregion

    #region Computed Properties

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void IsNotAuthenticated_ReturnsOppositeOfIsAuthenticated(bool isAuthenticated, bool expected)
    {
        var sut = CreateSut();
        sut.IsAuthenticated = isAuthenticated;

        Assert.Equal(expected, sut.IsNotAuthenticated);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Some error", true)]
    public void HasError_ReflectsErrorContent(string error, bool expected)
    {
        var sut = CreateSut();
        sut.Error = error;

        Assert.Equal(expected, sut.HasError);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsNotBusy_ReturnsOppositeOfIsBusy(bool isBusy)
    {
        var sut = CreateSut();
        sut.IsBusy = isBusy;

        Assert.Equal(!isBusy, sut.IsNotBusy);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public async Task MultipleLogins_WithDifferentUsers_UpdatesCorrectly()
    {
        var user1 = TestDataBuilder.Usuario.WithEmail("user1@example.com").WithName("User One").Build();
        var user2 = TestDataBuilder.Usuario.WithEmail("user2@example.com").WithName("User Two").Build();

        _usuarioService.LoginAsync(Arg.Any<CancellationToken>()).Returns(user1);
        var sut = CreateSut();

        await sut.LoginCommand.ExecuteAsync(null);
        var firstEmail = sut.Email;
        var firstName = sut.Name;

        _usuarioService.LoginAsync(Arg.Any<CancellationToken>()).Returns(user2);
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.Equal("user1@example.com", firstEmail),
            () => Assert.Equal("User One", firstName),
            () => Assert.Equal("user2@example.com", sut.Email),
            () => Assert.Equal("User Two", sut.Name));
    }

    #endregion
}
