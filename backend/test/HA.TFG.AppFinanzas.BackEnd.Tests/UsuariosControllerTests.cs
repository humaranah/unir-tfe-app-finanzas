using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;
using HA.TFG.AppFinanzas.BackEnd.Controllers;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Tests;

public class UsuariosControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly UsuariosController _sut;

    public UsuariosControllerTests()
    {
        _sut = new UsuariosController(_mediator);
    }

    private void SetUser(string? sub, string? email = "test@test.com", string? nombre = "Test User")
    {
        var claims = new List<Claim>();
        if (sub is not null) claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
        if (email is not null) claims.Add(new Claim("email", email));
        if (nombre is not null) claims.Add(new Claim("name", nombre));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
    }

    [Fact]
    public async Task Sync_UsuarioNuevo_DevuelveCreated()
    {
        // Arrange
        SetUser("auth0|123", "test@test.com", "Test User");
        var commandResult = new SyncUsuarioResult(1, "auth0|123", "test@test.com", "Test User", EsNuevo: true);

        _mediator.Send(Arg.Any<SyncUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        var result = await _sut.Sync(CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(commandResult, created.Value);
    }

    [Fact]
    public async Task Sync_UsuarioExistente_DevuelveOk()
    {
        // Arrange
        SetUser("auth0|456", "existente@test.com", "Existente");
        var commandResult = new SyncUsuarioResult(5, "auth0|456", "existente@test.com", "Existente", EsNuevo: false);

        _mediator.Send(Arg.Any<SyncUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        var result = await _sut.Sync(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(commandResult, ok.Value);
    }

    [Fact]
    public async Task Sync_SinClaimSub_DevuelveBadRequest()
    {
        // Arrange
        SetUser(null);

        // Act
        var result = await _sut.Sync(CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _mediator.DidNotReceive().Send(Arg.Any<SyncUsuarioCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_SinClaimEmail_DevuelveBadRequest()
    {
        // Arrange
        SetUser("auth0|123", email: null, nombre: "Test");

        // Act
        var result = await _sut.Sync(CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _mediator.DidNotReceive().Send(Arg.Any<SyncUsuarioCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_SinClaimNombre_DevuelveBadRequest()
    {
        // Arrange
        SetUser("auth0|123", email: "test@test.com", nombre: null);

        // Act
        var result = await _sut.Sync(CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _mediator.DidNotReceive().Send(Arg.Any<SyncUsuarioCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_EnviaCommandConDatosDelToken()
    {
        // Arrange
        const string idAuth0 = "auth0|999";
        const string email = "test@test.com";
        const string nombre = "Test User";
        SetUser(idAuth0, email, nombre);

        var commandResult = new SyncUsuarioResult(1, idAuth0, email, nombre, EsNuevo: true);
        _mediator.Send(Arg.Any<SyncUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        await _sut.Sync(CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<SyncUsuarioCommand>(c =>
                c.IdAuth0 == idAuth0 &&
                c.Email == email &&
                c.Nombre == nombre),
            Arg.Any<CancellationToken>());
    }
}
