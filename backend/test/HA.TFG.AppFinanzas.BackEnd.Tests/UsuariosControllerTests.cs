using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;
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

    private void SetUser(string? sub, string bearerToken = "test_token")
    {
        var claims = new List<Claim>();
        if (sub is not null) claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        httpContext.Request.Headers.Authorization = $"Bearer {bearerToken}";

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task Ensure_UsuarioNuevo_DevuelveCreated()
    {
        // Arrange
        SetUser("auth0|123");
        var commandResult = new EnsureUsuarioResult(1, "test@test.com", "Test User", null, true, null, EsNuevo: true);
        _mediator.Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>()).Returns(commandResult);

        // Act
        var result = await _sut.Ensure(CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(commandResult, created.Value);
    }

    [Fact]
    public async Task Ensure_UsuarioExistente_DevuelveOk()
    {
        // Arrange
        SetUser("auth0|456");
        var commandResult = new EnsureUsuarioResult(5, "existente@test.com", "Existente", null, true, null, EsNuevo: false);
        _mediator.Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>()).Returns(commandResult);

        // Act
        var result = await _sut.Ensure(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(commandResult, ok.Value);
    }

    [Fact]
    public async Task Ensure_SinClaimSub_DevuelveBadRequest()
    {
        // Arrange
        SetUser(null);

        // Act
        var result = await _sut.Ensure(CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _mediator.DidNotReceive().Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ensure_EnviaCommandConIdAuth0YAccessToken()
    {
        // Arrange
        const string idAuth0 = "auth0|999";
        const string token = "mi_token_jwt";
        SetUser(idAuth0, token);

        var commandResult = new EnsureUsuarioResult(1, "test@test.com", "Test User", null, true, null, EsNuevo: true);
        _mediator.Send(Arg.Any<EnsureUsuarioCommand>(), Arg.Any<CancellationToken>()).Returns(commandResult);

        // Act
        await _sut.Ensure(CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<EnsureUsuarioCommand>(c =>
                c.IdAuth0 == idAuth0 &&
                c.AccessToken == token),
            Arg.Any<CancellationToken>());
    }
}
