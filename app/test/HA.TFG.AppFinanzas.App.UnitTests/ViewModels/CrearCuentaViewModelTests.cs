using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Utilities;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class CrearCuentaViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();

    private CrearCuentaViewModel CreateSut() => new(_cuentasService);

    // ---------------------------------------------------------------------------
    // Estado inicial
    // ---------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsDefaultDescripcion()
    {
        var sut = CreateSut();

        Assert.Equal("Mis gastos", sut.Descripcion);
    }

    [Fact]
    public void Constructor_SetsDefaultMonedaSeleccionada()
    {
        var sut = CreateSut();

        Assert.Equal(MonedasHelper.DefaultMoneda, sut.MonedaSeleccionada);
    }

    [Fact]
    public void Constructor_MonedaSeleccionadaIsInMonedas()
    {
        var sut = CreateSut();

        Assert.Contains(sut.MonedaSeleccionada, sut.Monedas);
    }

    [Fact]
    public void Constructor_MonedasIsNotEmpty()
    {
        var sut = CreateSut();

        Assert.NotEmpty(sut.Monedas);
    }

    [Fact]
    public void Constructor_ErrorIsEmpty()
    {
        var sut = CreateSut();

        Assert.Empty(sut.Error);
        Assert.False(sut.HasError);
    }

    [Fact]
    public void Constructor_IsNotBusy()
    {
        var sut = CreateSut();

        Assert.False(sut.IsBusy);
        Assert.True(sut.IsNotBusy);
    }

    // ---------------------------------------------------------------------------
    // CrearCuentaAsync — caso feliz
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CrearCuentaAsync_WhenSuccessful_CallsServiceWithCorrectParameters()
    {
        var sut = CreateSut();
        sut.Descripcion = "Cuenta de vacaciones";
        sut.MonedaSeleccionada = new KeyValuePair<string, string>("USD", "USD – Us Dollar");

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        await _cuentasService.Received(1).CreateCuentaAsync(
            "Cuenta de vacaciones",
            "USD",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearCuentaAsync_WhenSuccessful_RaisesCuentaCreadaEvent()
    {
        var eventRaised = false;
        var sut = CreateSut();
        sut.CuentaCreada += (_, _) => eventRaised = true;

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.True(eventRaised);
    }

    [Fact]
    public async Task CrearCuentaAsync_WhenSuccessful_ErrorRemainsEmpty()
    {
        var sut = CreateSut();

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.Empty(sut.Error);
        Assert.False(sut.HasError);
    }

    [Fact]
    public async Task CrearCuentaAsync_WhenSuccessful_IsBusyIsFalseAfterCompletion()
    {
        var sut = CreateSut();

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.False(sut.IsBusy);
        Assert.True(sut.IsNotBusy);
    }

    // ---------------------------------------------------------------------------
    // CrearCuentaAsync — caso de error
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CrearCuentaAsync_WhenServiceThrows_SetsError()
    {
        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Error al crear cuenta. Status=500."));

        var sut = CreateSut();

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.NotEmpty(sut.Error);
        Assert.True(sut.HasError);
    }

    [Fact]
    public async Task CrearCuentaAsync_WhenServiceThrows_DoesNotRaiseCuentaCreadaEvent()
    {
        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Error al crear cuenta. Status=500."));

        var eventRaised = false;
        var sut = CreateSut();
        sut.CuentaCreada += (_, _) => eventRaised = true;

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.False(eventRaised);
    }

    [Fact]
    public async Task CrearCuentaAsync_WhenServiceThrows_IsBusyIsFalseAfterCompletion()
    {
        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Error al crear cuenta. Status=500."));

        var sut = CreateSut();

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.False(sut.IsBusy);
        Assert.True(sut.IsNotBusy);
    }

    [Fact]
    public async Task CrearCuentaAsync_WhenCalledAfterError_ClearsErrorBeforeAttempt()
    {
        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.NotEmpty(sut.Error);

        // Segunda llamada, ahora el servicio tiene éxito
        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.Empty(sut.Error);
    }
}
