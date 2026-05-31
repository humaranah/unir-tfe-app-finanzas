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
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly ProcesarComprobanteQueryHandler _sut;

    private static readonly Guid IdCuenta = Guid.CreateVersion7();
    private static readonly Guid IdUsuario = Guid.CreateVersion7();
    private const string Email = "usuario@test.com";

    private static readonly Usuario UsuarioDefault = new() { IdUsuario = IdUsuario, Email = Email };

    private static readonly IReadOnlyList<CuentaCategoria> CategoriasDefault =
    [
        new() { IdCuentaCategoria = Guid.CreateVersion7(), IdCuenta = IdCuenta, TipoMovimiento = TipoMovimiento.Gasto, Nombre = "Alimentación", Descripcion = "Supermercados y comida" },
        new() { IdCuentaCategoria = Guid.CreateVersion7(), IdCuenta = IdCuenta, TipoMovimiento = TipoMovimiento.Gasto, Nombre = "Transporte",   Descripcion = "Gasolina y transporte público" },
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
        _usuarioRepository.GetByEmailAsync(Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioDefault);

        _sut = new ProcesarComprobanteQueryHandler(
            _analysisService,
            _extraccionService,
            _usuarioRepository,
            _cuentaRepository);
    }

    private static ProcesarComprobanteQuery CrearQuery() => new()
    {
        ComprobanteStream = new MemoryStream([0x89, 0x50]),
        ContentType = "image/jpeg",
        IdCuenta = IdCuenta,
        Email = Email
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

        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
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

        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CuentaCategoria>());

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

        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
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

        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
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

        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
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

        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
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

        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonValido);

        await _sut.Handle(CrearQuery(), CancellationToken.None);

        await _extraccionService.Received(1).EnviarPromptAsync(
            Arg.Is<string>(p => p.Contains("Alimentación") && p.Contains("Transporte")),
            Arg.Any<CancellationToken>());
    }
}
