using CommunityToolkit.Mvvm.ComponentModel;
using HA.TFG.AppFinanzas.Core.Authentication;
using HA.TFG.AppFinanzas.Core.Models;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class UsuarioViewModelTests
{
    private readonly IUsuarioService _usuarioService = Substitute.For<IUsuarioService>();

    private UsuarioViewModel CreateSut() => new(_usuarioService);

    private static UsuarioInfo BuildInfo(string nombre = "Hugo", string email = "hugo@test.com") =>
        new(email, nombre, null, null, false, null);

    // ---------------------------------------------------------------------------
    // LoginAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task LoginAsync_WhenSuccessful_SetsUserAndAuthenticated()
    {
        _usuarioService.LoginAsync(Arg.Any<CancellationToken>()).Returns(BuildInfo());

        var sut = CreateSut();
        await sut.LoginCommand.ExecuteAsync(null);

        Assert.True(sut.IsAuthenticated);
        Assert.Equal("Hugo", sut.Name);
        Assert.Equal("hugo@test.com", sut.Email);
        Assert.Empty(sut.Error);
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

        Assert.False(sut.IsAuthenticated);
        Assert.Empty(sut.Error);
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
            () => Assert.False(sut.IsBusy)
        );
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
    public async Task LogoutAsync_CallsService()
    {
        var sut = CreateSut();
        await sut.LogoutCommand.ExecuteAsync(null);

        await _usuarioService.Received(1).LogoutAsync(Arg.Any<CancellationToken>());
    }

    // ---------------------------------------------------------------------------
    // TryRestoreSessionAsync
    // ---------------------------------------------------------------------------

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

        Assert.True(sut.IsAuthenticated);
        Assert.Equal("Hugo", sut.Name);
        Assert.Equal("hugo@test.com", sut.Email);
    }

}
