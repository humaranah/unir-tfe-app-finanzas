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
    private readonly ILlmService _extraccionService = Substitute.For<ILlmService>();
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaCategoriaRepository _cuentaCategoriaRepository = Substitute.For<ICuentaCategoriaRepository>();
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

    // JSON que el LLM devuelve con todos los campos (incluidos fallbacks para los campos DI)
    private static readonly string JsonLlmCompleto = """
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
            _cuentaCategoriaRepository);
    }

    private static ProcesarComprobanteQuery CrearQuery() => new()
    {
        ComprobanteStream = new MemoryStream([0x89, 0x50]),
        ContentType = "image/jpeg",
        IdCuenta = IdCuenta,
        Email = Email
    };

    private static ComprobanteAnalysisResult SoloTexto(string texto) =>
        new() { Texto = texto };

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
    public async Task Handle_AnalysisServiceDevuelveTextoVacioSinDatosEstructurados_LanzaExternalServiceException()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "   " });

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("DocumentIntelligence", ex.ServiceName);
    }

    [Fact]
    public async Task Handle_AnalysisServiceDevuelveSoloDatosEstructurados_ProcesaCorrectamente()
    {
        // Texto vacío pero con campos DI → debe continuar
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult { Texto = "", MerchantName = "HIPER ASIA", Total = 3.90m });

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto);

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("HIPER ASIA", result.Establecimiento);   // dato DI, no el del LLM
        Assert.Equal(3.90m, result.Importe);                  // dato DI
    }

    // ─── Paso 2: Repositorio de categorías ───────────────────────────────────

    [Fact]
    public async Task Handle_RepositorioLanzaExcepcion_LanzaExternalServiceExceptionBaseDeDatos()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(SoloTexto("Texto del ticket"));

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Fallo de BD"));

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("BaseDeDatos", ex.ServiceName);
    }

    [Fact]
    public async Task Handle_SinCategorias_LanzaExternalServiceExceptionBaseDeDatos()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(SoloTexto("Texto del ticket"));

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
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
            .Returns(SoloTexto("Texto del ticket"));

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
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
            .Returns(SoloTexto("Texto del ticket"));

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("esto no es json");

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("FoundryAI", ex.ServiceName);
    }

    // ─── Merge DI + LLM ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DiExtraeEstructurado_DatosDiTienenPrioridadSobreLlm()
    {
        var diResult = new ComprobanteAnalysisResult
        {
            Texto         = "HIPER ASIA\nTotal: 3.90",
            MerchantName  = "HIPER ASIA",
            Total         = 3.90m,
            Currency      = "PEN",
            TransactionDate = new DateOnly(2026, 5, 11),
            TransactionTime = new TimeOnly(14, 30, 0),
        };

        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(diResult);

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        // LLM devuelve valores distintos para los campos que ya extrajo DI
        const string jsonLlmConValoresDiferentes = """
            {
              "establecimiento": "OTRO COMERCIO",
              "concepto": "Artículos varios",
              "importe": 999.99,
              "moneda": "USD",
              "fechaMovimiento": "2020-01-01T00:00:00+00:00",
              "tipoMovimiento": "Gasto",
              "idCuentaCategoria": "c73ebca7-2bf5-fd6e-e041-642b86a9aa02",
              "nota": "En efectivo"
            }
            """;

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(jsonLlmConValoresDiferentes);

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        // Campos DI: prevalecen sobre el LLM
        Assert.Equal("HIPER ASIA", result.Establecimiento);
        Assert.Equal(3.90m, result.Importe);
        Assert.Equal("PEN", result.Moneda);
        Assert.Equal(new DateTimeOffset(2026, 5, 11, 14, 30, 0, TimeSpan.Zero), result.FechaMovimiento);

        // Campos inferidos por el LLM
        Assert.Equal("Artículos varios", result.Concepto);
        Assert.Equal("Gasto", result.TipoMovimiento);
        Assert.Equal("En efectivo", result.Nota);
    }

    [Fact]
    public async Task Handle_DiSinDatosEstructurados_UsaValoresDelLlm()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(SoloTexto("HIPER ASIA\nTotal: 3.90"));

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto);

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        // Sin datos DI, se usan los valores del LLM
        Assert.Equal("Hiper Asia", result.Establecimiento);
        Assert.Equal(3.90m, result.Importe);
        Assert.Equal("PEN", result.Moneda);
        Assert.Equal("Gasto", result.TipoMovimiento);
    }

    [Fact]
    public async Task Handle_FechaConHoraDi_ConstruyeDateTimeOffsetCorrectamente()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult
            {
                Texto = "Texto",
                TransactionDate = new DateOnly(2026, 3, 15),
                TransactionTime = new TimeOnly(10, 45, 0),
            });

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto);

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        Assert.Equal(new DateTimeOffset(2026, 3, 15, 10, 45, 0, TimeSpan.Zero), result.FechaMovimiento);
    }

    [Fact]
    public async Task Handle_FechaSinHoraDi_UsaMedioNoche()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult
            {
                Texto = "Texto",
                TransactionDate = new DateOnly(2026, 3, 15),
                // TransactionTime no extraída
            });

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto);

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        Assert.Equal(new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero), result.FechaMovimiento);
    }

    // ─── Flujo completo / regresión ───────────────────────────────────────────

    [Fact]
    public async Task Handle_FlujoCompleto_DevuelveComprobanteExtraidoDto()
    {
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(SoloTexto("HIPER ASIA\nTotal: 3.90"));

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto);

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
            .Returns(SoloTexto("HIPER ASIA\nTotal: 3.90"));

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns($"```json\n{JsonLlmCompleto}\n```");

        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        Assert.Equal("Hiper Asia", result.Establecimiento);
        Assert.Equal(3.90m, result.Importe);
    }

    // ─── Merge concepto: DI (single item) vs LLM ─────────────────────────────

    [Fact]
    public async Task Handle_WithSingleItemDescription_UsesDirectDescriptionIgnoringLlm()
    {
        // Arrange: DI extracts a single item with description
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult
            {
                Texto = "Ticket de compra",
                Items = [new ReceiptItemResult { Description = "Zapatillas deportivas", TotalPrice = 89.99m }]
            });

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        // LLM returns a different concepto; DI's should prevail
        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto); // contains "concepto": "Bolsa de regalo"

        // Act
        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        // Assert
        Assert.Equal("Zapatillas deportivas", result.Concepto);
    }

    [Fact]
    public async Task Handle_WithMultipleItemsOrSingleItemWithoutDescription_UsesLlmConcepto()
    {
        // Arrange: multiple items
        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ComprobanteAnalysisResult
            {
                Texto = "Ticket de compra",
                Items =
                [
                    new ReceiptItemResult { Description = "Pan", TotalPrice = 1.20m },
                    new ReceiptItemResult { Description = "Leche", TotalPrice = 0.90m },
                ]
            });

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto);

        // Act
        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        // Assert
        Assert.Equal("Bolsa de regalo", result.Concepto);
    }

    [Fact]
    public async Task Handle_ElPromptContieneCategoriasYDatosDi_PromptEnviadoAFoundry()
    {
        var diResult = new ComprobanteAnalysisResult
        {
            Texto        = "Texto del ticket",
            MerchantName = "SUPERMERCADO XYZ",
            Total        = 25.50m,
            Currency     = "EUR",
        };

        _analysisService.AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(diResult);

        _cuentaCategoriaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CategoriasDefault);

        _extraccionService.EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(JsonLlmCompleto);

        await _sut.Handle(CrearQuery(), CancellationToken.None);

        await _extraccionService.Received(1).EnviarPromptAsync(
            Arg.Is<string>(p =>
                p.Contains("Alimentación") &&
                p.Contains("Transporte") &&
                p.Contains("SUPERMERCADO XYZ") &&
                p.Contains("25.50") &&
                p.Contains("EUR")),
            Arg.Any<CancellationToken>());
    }
}
