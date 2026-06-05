using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Recomendaciones;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class RecomendacionesViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();
    private readonly IRecomendacionesService _recomendacionesService = Substitute.For<IRecomendacionesService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();

    private RecomendacionesViewModel CreateSut() => new(_cuentasService, _recomendacionesService);

    private void ConfigurarCuentaPorDefecto()
        => _cuentasService.GetDefaultCuentaAsync(Arg.Any<CancellationToken>()).Returns((IdCuenta, "Mi Cuenta"));

    private void ConfigurarSinCuentaPorDefecto()
        => _cuentasService.GetDefaultCuentaAsync(Arg.Any<CancellationToken>()).Returns(((Guid?)null, (string?)null));

    private static RecomendacionResult CrearRecomendacion(string content = "Recomendación de prueba")
        => new() { Content = content, GeneratedAt = DateTimeOffset.Now };

    // ── Estado Inicial ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_InitializesWithDefaultState()
    {
        var sut = CreateSut();

        Assert.False(sut.IsBusy);
        Assert.Empty(sut.Content);
        Assert.Empty(sut.Error);
        Assert.Empty(sut.Query);
        Assert.False(sut.HasContent);
        Assert.False(sut.HasError);
        Assert.True(sut.IsNotBusy);
    }

    // ── CargarResumenAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CargarResumenAsync_WhenSuccessful_LoadsRecommendation()
    {
        var recomendacion = CrearRecomendacion("Este es un resumen de tus gastos");
        ConfigurarCuentaPorDefecto();
        _recomendacionesService.GetRecomendacionAsync(IdCuenta, null, Arg.Any<CancellationToken>())
            .Returns(recomendacion);

        var sut = CreateSut();
        await sut.CargarResumenCommand.ExecuteAsync(null);

        Assert.Equal(recomendacion.Content, sut.Content);
        Assert.Empty(sut.Error);
        Assert.False(sut.IsBusy);
        Assert.True(sut.HasContent);
    }

    [Fact]
    public async Task CargarResumenAsync_WhenNoDefaultAccount_SetsError()
    {
        ConfigurarSinCuentaPorDefecto();

        var sut = CreateSut();
        await sut.CargarResumenCommand.ExecuteAsync(null);

        Assert.NotEmpty(sut.Error);
        Assert.Empty(sut.Content);
        Assert.False(sut.IsBusy);
        _ = _recomendacionesService.DidNotReceive()
            .GetRecomendacionAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CargarResumenAsync_WhenServiceThrows_CapturesError()
    {
        ConfigurarCuentaPorDefecto();
        _recomendacionesService.GetRecomendacionAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Error del servidor"));

        var sut = CreateSut();
        await sut.CargarResumenCommand.ExecuteAsync(null);

        Assert.NotEmpty(sut.Error);
        Assert.False(sut.IsBusy);
    }

    // ── PreguntarAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task PreguntarAsync_WhenSuccessful_LoadsResponse()
    {
        var recomendacion = CrearRecomendacion("Respuesta a tu pregunta");
        ConfigurarCuentaPorDefecto();
        _recomendacionesService.GetRecomendacionAsync(IdCuenta, "¿Cómo ahorrar dinero?", Arg.Any<CancellationToken>())
            .Returns(recomendacion);

        var sut = CreateSut();
        sut.Query = "¿Cómo ahorrar dinero?";
        await sut.PreguntarCommand.ExecuteAsync(null);

        Assert.Equal(recomendacion.Content, sut.Content);
        Assert.Empty(sut.Query);
        Assert.Empty(sut.Error);
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task PreguntarAsync_WhenQueryIsEmpty_DoesNotCallService()
    {
        var sut = CreateSut();
        sut.Query = string.Empty;
        await sut.PreguntarCommand.ExecuteAsync(null);

        _ = _recomendacionesService.DidNotReceive()
            .GetRecomendacionAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PreguntarAsync_WhenServiceThrows_CapturesError()
    {
        ConfigurarCuentaPorDefecto();
        _recomendacionesService.GetRecomendacionAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Error al procesar"));

        var sut = CreateSut();
        sut.Query = "Mi pregunta";
        await sut.PreguntarCommand.ExecuteAsync(null);

        Assert.NotEmpty(sut.Error);
        Assert.False(sut.IsBusy);
    }

    // ── Comandos ──────────────────────────────────────────────────────────────

    [Fact]
    public void Commands_RespectIsBusyState()
    {
        var sut = CreateSut();

        Assert.True(sut.CargarResumenCommand.CanExecute(null));
        Assert.True(sut.PreguntarCommand.CanExecute(null));

        sut.IsBusy = true;

        Assert.False(sut.CargarResumenCommand.CanExecute(null));
        Assert.False(sut.PreguntarCommand.CanExecute(null));
    }

    // ── Propiedades Observables ───────────────────────────────────────────────

    [Fact]
    public async Task ObservableProperties_UpdateCorrectly()
    {
        var recomendacion = CrearRecomendacion("Contenido");
        ConfigurarCuentaPorDefecto();
        _recomendacionesService.GetRecomendacionAsync(IdCuenta, null, Arg.Any<CancellationToken>())
            .Returns(recomendacion);

        var sut = CreateSut();
        Assert.False(sut.HasContent);
        Assert.False(sut.HasError);
        Assert.True(sut.IsNotBusy);

        await sut.CargarResumenCommand.ExecuteAsync(null);

        Assert.True(sut.HasContent);
        Assert.False(sut.HasError);
        Assert.True(sut.IsNotBusy);
    }

    [Fact]
    public async Task AccountIsLoadedOnce()
    {
        var recomendacion = CrearRecomendacion();
        ConfigurarCuentaPorDefecto();
        _recomendacionesService.GetRecomendacionAsync(IdCuenta, null, Arg.Any<CancellationToken>())
            .Returns(recomendacion);

        var sut = CreateSut();
        await sut.CargarResumenCommand.ExecuteAsync(null);
        await sut.CargarResumenCommand.ExecuteAsync(null);

        await _cuentasService.Received(1).GetDefaultCuentaAsync(Arg.Any<CancellationToken>());
    }
}
