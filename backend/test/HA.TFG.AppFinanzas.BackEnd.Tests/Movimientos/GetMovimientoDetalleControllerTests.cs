using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetComprobanteMovimientoQuery;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientoDetalleQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Movimientos;

public class GetMovimientoDetalleControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CuentasController _sut;

    private static readonly Guid IdCuenta = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid IdMovimiento = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private const string Email = "test@test.com";

    public GetMovimientoDetalleControllerTests()
    {
        _sut = new CuentasController(_mediator);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, Email)]))
            }
        };
    }

    [Fact]
    public async Task GetMovimientoDetalle_DevuelveOk_ConResultado()
    {
        // Arrange
        var detalle = new GetMovimientoDetalleResult { IdMovimiento = IdMovimiento, Concepto = "Taxi" };
        _mediator.Send(Arg.Any<GetMovimientoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(detalle);

        // Act
        var result = await _sut.GetMovimientoDetalle(IdCuenta, IdMovimiento, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(detalle, ok.Value);
    }

    [Fact]
    public async Task GetMovimientoDetalle_EnviaQueryConParametrosCorrectos()
    {
        // Arrange
        _mediator.Send(Arg.Any<GetMovimientoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetMovimientoDetalleResult());

        // Act
        await _sut.GetMovimientoDetalle(IdCuenta, IdMovimiento, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetMovimientoDetalleQuery>(q =>
                q.Email == Email &&
                q.IdCuenta == IdCuenta &&
                q.IdMovimiento == IdMovimiento),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetComprobanteMovimiento_DevuelveFile_CuandoComprobanteExiste()
    {
        // Arrange
        await using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);
        var comprobante = new ComprobanteFile(stream, "application/pdf", "recibo.pdf");

        _mediator.Send(Arg.Any<GetComprobanteMovimientoQuery>(), Arg.Any<CancellationToken>())
            .Returns(comprobante);

        // Act
        var result = await _sut.GetComprobanteMovimiento(IdCuenta, IdMovimiento, CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("recibo.pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task GetComprobanteMovimiento_EnviaQueryConParametrosCorrectos()
    {
        // Arrange
        await using var stream = new MemoryStream();
        _mediator.Send(Arg.Any<GetComprobanteMovimientoQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteFile(stream, "application/octet-stream", "file.bin"));

        // Act
        await _sut.GetComprobanteMovimiento(IdCuenta, IdMovimiento, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetComprobanteMovimientoQuery>(q =>
                q.Email == Email &&
                q.IdCuenta == IdCuenta &&
                q.IdMovimiento == IdMovimiento),
            Arg.Any<CancellationToken>());
    }
}
