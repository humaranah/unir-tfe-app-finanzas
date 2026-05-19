using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Tests;

public class GetCategoriasCuentaControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CuentasController _sut;

    private static readonly Guid IdCuenta = Guid.Parse("00000000-0000-7000-8000-000000000010");

    public GetCategoriasCuentaControllerTests()
    {
        _sut = new CuentasController(_mediator);
        SetUser("test@test.com");
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
    public async Task GetCategoriasCuenta_DevuelveOk_ConListaDeCategorias()
    {
        // Arrange
        var categorias = new List<GetCuentaCategoriasResultItem>
        {
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Nómina" },
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Supermercado" }
        };
        _mediator.Send(Arg.Any<GetCuentaCategoriasQuery>(), Arg.Any<CancellationToken>())
            .Returns(categorias);

        // Act
        var result = await _sut.GetCategoriasCuenta(IdCuenta, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(categorias, ok.Value);
    }

    [Fact]
    public async Task GetCategoriasCuenta_EnviaQueryConEmailEIdCuentaCorrectos()
    {
        // Arrange
        const string email = "test@test.com";
        SetUser(email);
        _mediator.Send(Arg.Any<GetCuentaCategoriasQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<GetCuentaCategoriasResultItem>());

        // Act
        await _sut.GetCategoriasCuenta(IdCuenta, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetCuentaCategoriasQuery>(q => q.Email == email && q.IdCuenta == IdCuenta),
            Arg.Any<CancellationToken>());
    }
}
