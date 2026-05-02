using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;
using HA.TFG.AppFinanzas.BackEnd.Controllers;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;
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

    private static EnsureUsuarioRequest DefaultRequest(string email = "test@test.com", string nombre = "Test User") =>
        new(email, nombre, null, true, null);

    [Fact]
    public async Task Sync_UsuarioNuevo_DevuelveCreated()
    {
        // Arrange
        SetUser("auth0|123", "test@test.com", "Test User");
        var commandResult = new EnsureUsuarioResult(1, "auth0|123", "test@test.com", "Test User", null, null, true, null, EsNuevo: true);

        _mediator.Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        var result = await _sut.Ensure(DefaultRequest(), CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(commandResult, created.Value);
    }

    [Fact]
    public async Task Sync_UsuarioExistente_DevuelveOk()
    {
        // Arrange
        SetUser("auth0|456", "existente@test.com", "Existente");
        var commandResult = new EnsureUsuarioResult(5, "auth0|456", "existente@test.com", "Existente", null, null, true, null, EsNuevo: false);

        _mediator.Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        var result = await _sut.Ensure(DefaultRequest("existente@test.com", "Existente"), CancellationToken.None);

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
        var result = await _sut.Ensure(DefaultRequest(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _mediator.DidNotReceive().Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_EnviaCommandConDatosDelTokenYBody()
    {
        // Arrange
        const string idAuth0 = "auth0|999";
        const string email = "test@test.com";
        const string nombre = "Test User";
        SetUser(idAuth0, email, nombre);

        var commandResult = new EnsureUsuarioResult(1, idAuth0, email, nombre, null, null, true, null, EsNuevo: true);
        _mediator.Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        var request = new EnsureUsuarioRequest(email, nombre, null, true, null);

        // Act
        await _sut.Ensure(request, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<EnsureUsuarioCommand>(c =>
                c.IdAuth0 == idAuth0 &&
                c.Email == email &&
                c.Nombre == nombre),
            Arg.Any<CancellationToken>());
    }
}
