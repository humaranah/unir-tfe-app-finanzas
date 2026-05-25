using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class ProcesarComprobanteQueryHandlerTests
{
    private readonly IComprobanteAnalysisService _analysisService = Substitute.For<IComprobanteAnalysisService>();
    private readonly IComprobanteExtraccionService _extraccionService = Substitute.For<IComprobanteExtraccionService>();
    private readonly ICategoriaRepository _categoriaRepository = Substitute.For<ICategoriaRepository>();
    private readonly ProcesarComprobanteQueryHandler _sut;

    private static readonly IReadOnlyList<Categoria> CategoriasDefault =
    [
        Categoria.CrearNuevo(TipoMovimiento.Gasto, "Alimentación", "Supermercados y comida"),
        Categoria.CrearNuevo(TipoMovimiento.Gasto, "Transporte",   "Gasolina y transporte público"),
    ];

    private static readonly string JsonValido = """
        {
          "establecimiento": "Hiper Asia",
          "concepto": "Bolsa de regalo",
          "importe": 3.90,
          "moneda": "PEN",
          "fechaMovimiento": "2026-05-11T00:00:00+00:00",
          "tipoMovimiento": "Gasto",
          "idCuentaCategoria": "c73ebca7-2bf5-fd6e-e041-642b86a9aa02",
          "nota": "Pagado con VISA"
        }
        """;

    public ProcesarComprobanteQueryHandlerTests()
    {
        _sut = new ProcesarComprobanteQueryHandler(
            _analysisService,
            _extraccionService,
            _categoriaRepository);
    }

    private static ProcesarComprobanteQuery CrearQuery() => new()
    {
        ComprobanteStream = new MemoryStream([0x89, 0x50]),
        ContentType = "image/jpeg"
    };

    // ─── Paso 1: Document Intelligence ───────────────────────────────────────

    [Fact]
    public async Task Handle_AnalysisServiceDevuelveNull_LanzaExternalServiceException()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ComprobanteAnalysisResult?)null);

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("DocumentIntelligence", ex.ServiceName);
    }

    [Fact]
    public async Task Handle_AnalysisServiceDevuelveTextoVacio_LanzaExternalServiceException()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "   " });

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("DocumentIntelligence", ex.ServiceName);
    }

    // ─── Paso 2: Repositorio de categorías ───────────────────────────────────

    [Fact]
    public async Task Handle_RepositorioLanzaExcepcion_LanzaExternalServiceExceptionBaseDeDatos()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "Texto del ticket" });

        _categoriaRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Fallo de BD"));

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("BaseDeDatos", ex.ServiceName);
    }

    [Fact]
    public async Task Handle_SinCategorias_LanzaExternalServiceExceptionBaseDeDatos()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "Texto del ticket" });

        _categoriaRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Categoria>());

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("BaseDeDatos", ex.ServiceName);
    }

    // ─── Paso 4: Foundry AI ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FoundryDevuelveVacio_LanzaExternalServiceExceptionFoundryAI()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "Texto del ticket" });

        _categoriaRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("FoundryAI", ex.ServiceName);
    }

    [Fact]
    public async Task Handle_FoundryDevuelveJsonInvalido_LanzaExternalServiceExceptionFoundryAI()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "Texto del ticket" });

        _categoriaRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("esto no es json");

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("FoundryAI", ex.ServiceName);
    }

    // ─── Flujo completo ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FlujoCompleto_DevuelveComprobanteExtraidoDto()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "HIPER ASIA\nTotal: 3.90" });

        _categoriaRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonValido);

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        Assert.Equal("Hiper Asia", result.Establecimiento);
        Assert.Equal(3.90m, result.Importe);
        Assert.Equal("PEN", result.Moneda);
        Assert.Equal("Gasto", result.TipoMovimiento);
    }

    [Fact]
    public async Task Handle_FoundryDevuelveJsonEnMarkdown_DeserializaCorrectamente()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "HIPER ASIA\nTotal: 3.90" });

        _categoriaRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns($"```json\n{JsonValido}\n```");

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        Assert.Equal("Hiper Asia", result.Establecimiento);
        Assert.Equal(3.90m, result.Importe);
    }

    [Fact]
    public async Task Handle_ElPromptContieneCategorias_PromptEnviadoAFoundry()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "Texto del ticket" });

        _categoriaRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonValido);

        await _sut.Handle(CrearQuery(), CancellationToken.None);

        await _extraccionService.Received(1).EnviarPromptAsync(
            Arg.Is<string>(p => p.Contains("Alimentación") && p.Contains("Transporte")),
            Arg.Any<CancellationToken>());
    }
}
