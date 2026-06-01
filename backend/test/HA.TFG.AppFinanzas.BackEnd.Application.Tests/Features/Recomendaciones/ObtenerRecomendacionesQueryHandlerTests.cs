using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Recomendaciones.ObtenerRecomendacionesQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Recomendaciones;

public class ObtenerRecomendacionesQueryHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly IMovimientoRepository _movimientoRepository = Substitute.For<IMovimientoRepository>();
    private readonly ILlmService _llmService = Substitute.For<ILlmService>();
    private readonly ObtenerRecomendacionesQueryHandler _sut;

    private static readonly Guid IdCuenta = Guid.CreateVersion7();
    private static readonly Guid IdUsuario = Guid.CreateVersion7();
    private const string Email = "usuario@test.com";
    private const string RespuestaLlm = "Tus gastos en Alimentación han subido un 15% este mes. Considera reducir comidas fuera.";

    private static readonly Usuario UsuarioDefault = new() { IdUsuario = IdUsuario, Email = Email };
    private static readonly Cuenta CuentaDefault = new() { IdCuenta = IdCuenta, Moneda = "EUR", Descripcion = "Cuenta principal" };

    private static readonly IReadOnlyList<ResumenGastoCategoria> ResumenMesActual =
    [
        new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, Guid.CreateVersion7(), "Alimentación", "EUR", 320.00m),
        new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, Guid.CreateVersion7(), "Transporte",   "EUR",  85.00m),
    ];

    private static readonly IReadOnlyList<ResumenGastoCategoria> ResumenHistorico =
    [
        new(DateTime.UtcNow.Year, DateTime.UtcNow.AddMonths(-1).Month, Guid.CreateVersion7(), "Alimentación", "EUR", 280.00m),
        new(DateTime.UtcNow.Year, DateTime.UtcNow.AddMonths(-1).Month, Guid.CreateVersion7(), "Transporte",   "EUR",  75.00m),
    ];

    public ObtenerRecomendacionesQueryHandlerTests()
    {
        _usuarioRepository.GetByEmailAsync(Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioDefault);

        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaDefault);

        _movimientoRepository
            .GetResumenGastosPorCategoriaAsync(IdCuenta, Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(ResumenMesActual);

        _llmService
            .EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<string?>())
            .Returns(RespuestaLlm);

        _sut = new ObtenerRecomendacionesQueryHandler(
            _usuarioRepository,
            _cuentaRepository,
            _movimientoRepository,
            _llmService,
            NullLogger<ObtenerRecomendacionesQueryHandler>.Instance);
    }

    private static ObtenerRecomendacionesQuery CrearQuery(string? consulta = null) => new()
    {
        IdCuenta     = IdCuenta,
        EmailUsuario = Email,
        Consulta     = consulta
    };

    // ─── Paso 1: resolución de usuario y cuenta ──────────────────────────────

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        _usuarioRepository.GetByEmailAsync(Email, Arg.Any<CancellationToken>())
            .Returns((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_CuentaNoExiste_LanzaNotFoundException()
    {
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns((Cuenta?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());
    }

    // ─── Paso 5: llamada al LLM ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_LlmDevuelveVacio_LanzaExternalServiceExceptionFoundryAI()
    {
        _llmService
            .EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<string?>())
            .Returns(string.Empty);

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("FoundryAI", ex.ServiceName);
    }

    [Fact]
    public async Task Handle_LlmDevuelveNull_LanzaExternalServiceExceptionFoundryAI()
    {
        _llmService
            .EnviarPromptAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<string?>())
            .Returns((string?)null);

        var ex = await Assert.ThrowsAsync<ExternalServiceException>(() =>
            _sut.Handle(CrearQuery(), CancellationToken.None).AsTask());

        Assert.Equal("FoundryAI", ex.ServiceName);
    }

    // ─── Flujo completo ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FlujoCompleto_DevuelveRecomendacionResult()
    {
        var result = await _sut.Handle(CrearQuery(), CancellationToken.None);

        Assert.Equal(RespuestaLlm, result.Contenido);
        Assert.True(result.GeneradoEn <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_SinConsulta_PromptContieneInstruccionGenerica()
    {
        await _sut.Handle(CrearQuery(consulta: null), CancellationToken.None);

        await _llmService.Received(1).EnviarPromptAsync(
            Arg.Is<string>(p => p.Contains("no ha indicado una consulta específica")),
            Arg.Any<CancellationToken>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task Handle_ConConsulta_PromptContieneTextoDelUsuario()
    {
        const string consulta = "¿Estoy gastando demasiado en ocio?";

        await _sut.Handle(CrearQuery(consulta: consulta), CancellationToken.None);

        await _llmService.Received(1).EnviarPromptAsync(
            Arg.Is<string>(p => p.Contains(consulta)),
            Arg.Any<CancellationToken>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task Handle_ConDatosMesActual_PromptContieneCategoriasDelMes()
    {
        await _sut.Handle(CrearQuery(), CancellationToken.None);

        await _llmService.Received(1).EnviarPromptAsync(
            Arg.Is<string>(p => p.Contains("Alimentación") && p.Contains("Transporte")),
            Arg.Any<CancellationToken>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task Handle_LlmRecibeLasInstruccionesDeAsesor()
    {
        await _sut.Handle(CrearQuery(), CancellationToken.None);

        await _llmService.Received(1).EnviarPromptAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>(),
            Arg.Is<string?>(i => i != null && i.Contains("asesor financiero")));
    }

    [Fact]
    public async Task Handle_SinHistorico_PromptIndicaQueNoHayDatos()
    {
        // Primera llamada (mes actual) devuelve datos; segunda (histórico) devuelve vacío.
        IReadOnlyList<ResumenGastoCategoria> sinHistorico = [];
        _movimientoRepository
            .GetResumenGastosPorCategoriaAsync(IdCuenta, Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(ResumenMesActual, sinHistorico);

        await _sut.Handle(CrearQuery(), CancellationToken.None);

        await _llmService.Received(1).EnviarPromptAsync(
            Arg.Is<string>(p => p.Contains("Sin historial de gastos disponible")),
            Arg.Any<CancellationToken>(),
            Arg.Any<string?>());
    }
}
