using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using HA.TFG.AppFinanzas.Core.Utilities;
using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.Core.Tests.ViewModels;

public class CrearCuentaViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();

    private CrearCuentaViewModel CreateSut() => new(_cuentasService);

    #region Estado inicial

    [Fact]
    public void Constructor_InitialState_MatchesExpectedSnapshot()
    {
        var sut = CreateSut();

        Assert.Multiple(
            () => Assert.Equal("Mis gastos", sut.Descripcion),
            () => Assert.Equal(MonedasHelper.DefaultMoneda, sut.MonedaSeleccionada),
            () => Assert.Contains(sut.MonedaSeleccionada, sut.Monedas),
            () => Assert.NotEmpty(sut.Monedas),
            () => Assert.Empty(sut.Error),
            () => Assert.False(sut.HasError),
            () => Assert.False(sut.IsBusy),
            () => Assert.True(sut.IsNotBusy));
    }

    #endregion

    #region CrearCuentaAsync — caso feliz

    [Fact]
    public async Task CrearCuentaAsync_WhenSuccessful_CallsServiceWithCorrectParameters()
    {
        var sut = CreateSut();
        sut.Descripcion = "Cuenta de vacaciones";
        sut.MonedaSeleccionada = new KeyValuePair<string, string>("USD", "USD – Us Dollar");

        await sut.CrearCuentaCommand.ExecuteAsync(null);

        await _cuentasService.Received(1).CreateCuentaAsync(
            "Cuenta de vacaciones", "USD", Arg.Any<CancellationToken>());
        Assert.Multiple(
            () => Assert.Empty(sut.Error),
            () => Assert.False(sut.HasError),
            () => Assert.False(sut.IsBusy));
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

    #endregion

    #region CrearCuentaAsync — caso de error

    [Fact]
    public async Task CrearCuentaAsync_WhenServiceThrows_SetsError()
    {
        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Error al crear cuenta. Status=500."));

        var sut = CreateSut();
        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.NotEmpty(sut.Error),
            () => Assert.True(sut.HasError),
            () => Assert.False(sut.IsBusy));
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
    public async Task CrearCuentaAsync_WhenCalledAfterError_ClearsErrorBeforeAttempt()
    {
        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CrearCuentaCommand.ExecuteAsync(null);
        Assert.NotEmpty(sut.Error);

        _cuentasService
            .CreateCuentaAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        await sut.CrearCuentaCommand.ExecuteAsync(null);

        Assert.Empty(sut.Error);
    }

    #endregion
}
