using HA.TFG.AppFinanzas.BackEnd.Application;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;
using HA.TFG.AppFinanzas.BackEnd.Controllers;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Movimientos;

public class ComprobantesControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ComprobantesController _sut;

    public ComprobantesControllerTests()
    {
        var options = Options.Create(new ComprobanteConfig());
        _sut = new ComprobantesController(_mediator, options);
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
        var result = await _sut.EscanearComprobante(file, CancellationToken.None);

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
        await _sut.EscanearComprobante(file, CancellationToken.None);

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
        await _sut.EscanearComprobante(file, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<ProcesarComprobanteQuery>(q => q.ComprobanteStream != null),
            Arg.Any<CancellationToken>());
    }
}
