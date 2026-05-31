using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Movimientos;

public class ComprobantesControllerTests
{
    private const string Email = "usuario@test.com";

    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ComprobantesController _sut;

    public ComprobantesControllerTests()
    {
        _sut = new ComprobantesController(_mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.Name, Email)], "TestAuth"))
                }
            }
        };
    }

    private static IFormFile CrearFormFile(string contentType = "image/jpeg", int size = 512)
    {
        var file = Substitute.For<IFormFile>();
        var stream = new MemoryStream(new byte[size]);
        file.ContentType.Returns(contentType);
        file.Length.Returns(size);
        file.OpenReadStream().Returns(stream);
        return file;
    }

    [Fact]
    public async Task EscanearComprobante_DevuelveOk_ConResultadoDelHandler()
    {
        // Arrange
        var file = CrearFormFile();
        var dto = new ComprobanteExtraidoDto { Establecimiento = "Supermercado Test", Importe = 12.50m };
        _mediator.Send(Arg.Any<ProcesarComprobanteQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);

        // Act
        var result = await _sut.EscanearComprobante(file, Guid.CreateVersion7(), CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task EscanearComprobante_EnviaQueryConContentTypeDelArchivo()
    {
        // Arrange
        const string contentType = "application/pdf";
        var file = CrearFormFile(contentType);
        _mediator.Send(Arg.Any<ProcesarComprobanteQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteExtraidoDto());

        // Act
        await _sut.EscanearComprobante(file, Guid.CreateVersion7(), CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<ProcesarComprobanteQuery>(q => q.ContentType == contentType),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EscanearComprobante_EnviaQueryConStreamDelArchivo()
    {
        // Arrange
        var file = CrearFormFile();
        _mediator.Send(Arg.Any<ProcesarComprobanteQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteExtraidoDto());

        // Act
        await _sut.EscanearComprobante(file, Guid.CreateVersion7(), CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<ProcesarComprobanteQuery>(q => q.ComprobanteStream != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EscanearComprobante_EnviaQueryConIdCuentaYEmail()
    {
        // Arrange
        var idCuenta = Guid.CreateVersion7();
        var file = CrearFormFile();
        _mediator.Send(Arg.Any<ProcesarComprobanteQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteExtraidoDto());

        // Act
        await _sut.EscanearComprobante(file, idCuenta, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<ProcesarComprobanteQuery>(q => q.IdCuenta == idCuenta && q.Email == Email),
            Arg.Any<CancellationToken>());
    }
}
