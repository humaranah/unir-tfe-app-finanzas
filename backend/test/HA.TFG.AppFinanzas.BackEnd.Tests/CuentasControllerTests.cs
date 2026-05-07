using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Tests;

public class CuentasControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CuentasController _sut;

    public CuentasControllerTests()
    {
        _sut = new CuentasController(_mediator);
    }

    private void SetUser(string? name)
    {
        var claims = new List<Claim>();
        if (name is not null) claims.Add(new Claim(ClaimTypes.Name, name));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
    }

    [Fact]
    public async Task GetCuentas_DevuelveOk_ConResultado()
    {
        // Arrange
        SetUser("test@test.com");
        var cuentas = new List<GetCuentasResultItem>
        {
            new() { Id = 1, Nombre = "Cuenta A", Descripcion = "Desc A" }
        };
        _mediator.Send(Arg.Any<GetCuentasQuery>(), Arg.Any<CancellationToken>()).Returns(cuentas);

        // Act
        var result = await _sut.GetCuentas(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cuentas, ok.Value);
    }

    [Fact]
    public async Task GetCuentas_EnviaQueryConEmailDelUsuario()
    {
        // Arrange
        const string email = "test@test.com";
        SetUser(email);
        _mediator.Send(Arg.Any<GetCuentasQuery>(), Arg.Any<CancellationToken>()).Returns(new List<GetCuentasResultItem>());

        // Act
        await _sut.GetCuentas(CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetCuentasQuery>(q => q.Email == email),
            Arg.Any<CancellationToken>());
    }
}
