using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Features.Movimientos;
using HA.TFG.AppFinanzas.Core.Features.Movimientos.Models;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.Services;
using HA.TFG.AppFinanzas.Core.Tests.Fixtures;
using HA.TFG.AppFinanzas.Core.Features.Movimientos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.Core.Tests.ViewModels;

public class MovimientosViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();
    private readonly IMovimientosService _movimientosService = Substitute.For<IMovimientosService>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly IConfirmationService _confirmationService = Substitute.For<IConfirmationService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();

    private MovimientosViewModel CreateSut() => new(_cuentasService, _movimientosService, _navigationService, _confirmationService);

    private void ConfigurarCuenta(string descripcion = "Mi cuenta")
        => _cuentasService.GetDefaultCuentaAsync().Returns((IdCuenta, (string?)descripcion));

    private void ConfigurarSinCuenta()
        => _cuentasService.GetDefaultCuentaAsync().Returns((null, null));

    private static MovimientoItem CrearMovimiento(DateOnly fecha, decimal importe = 10m)
        => TestDataBuilder.Movimiento
            .WithIdCuenta(IdCuenta)
            .WithMoneda("EUR")
            .WithDate(fecha)
            .WithAmount(importe)
            .Build();

    #region Estado inicial

    [Fact]
    public void Constructor_InitialState_MatchesExpectedSnapshot()
    {
        var now = DateTime.Now;
        var sut = CreateSut();

        Assert.Multiple(
            () => Assert.False(sut.IsBusy),
            () => Assert.Empty(sut.Movimientos),
            () => Assert.Equal(string.Empty, sut.Error),
            () => Assert.Equal(string.Empty, sut.NombreCuenta),
            () => Assert.Equal(now.Year, sut.Fecha.Year),
            () => Assert.Equal(now.Month, sut.Fecha.Month),
            () => Assert.True(sut.SinMovimientos),
            () => Assert.False(sut.HasMovimientos),
            () => Assert.False(sut.HasError));
    }

    #endregion

    #region SinMovimientos / HasMovimientos

    [Fact]
    public async Task CargarMovimientosAsync_WhenMovimientosLoaded_UpdatesMovimientosFlags()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([CrearMovimiento(DateOnly.FromDateTime(DateTime.Now))]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Multiple(
            () => Assert.False(sut.SinMovimientos),
            () => Assert.True(sut.HasMovimientos));
    }

    [Fact]
    public async Task CargarMovimientosAsync_WhenHasError_MovimientosFlagsAreFalse()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Multiple(
            () => Assert.False(sut.SinMovimientos),
            () => Assert.False(sut.HasMovimientos));
    }

    #endregion

    #region HasError

    [Fact]
    public async Task HasError_WhenServiceThrows_IsTrue()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.True(sut.HasError);
    }

    [Fact]
    public async Task HasError_IsClearedOnNextSuccessfulLoad()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.True(sut.HasError);

        _cuentasService.GetDefaultCuentaAsync(Arg.Any<CancellationToken>())
            .Returns((IdCuenta, (string?)"Mi cuenta"));
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([]);

        await sut.CargarMovimientosAsync();

        Assert.False(sut.HasError);
    }

    #endregion

    #region CargarMovimientosAsync

    [Fact]
    public async Task CargarMovimientosAsync_WhenNoCuenta_MovimientosIsEmpty()
    {
        ConfigurarSinCuenta();

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Empty(sut.Movimientos);
    }

    [Fact]
    public async Task CargarMovimientosAsync_WhenSuccessful_PopulatesMovimientos()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([CrearMovimiento(hoy, 50m), CrearMovimiento(hoy, 30m)]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Multiple(
            () => Assert.NotEmpty(sut.Movimientos),
            () => Assert.Equal(2, sut.Movimientos.Sum(g => g.Count())),
            () => Assert.False(sut.IsBusy));
    }

    [Fact]
    public async Task CargarMovimientosAsync_WhenSuccessful_SetsNombreCuenta()
    {
        ConfigurarCuenta("Cuenta principal");
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Equal("Cuenta principal", sut.NombreCuenta);
    }

    [Fact]
    public async Task CargarMovimientosAsync_WhenServiceThrows_SetsError()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("Error de red"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Multiple(
            () => Assert.Equal("Error de red", sut.Error),
            () => Assert.True(sut.HasError),
            () => Assert.False(sut.IsBusy));
    }

    [Fact]
    public async Task CargarMovimientosAsync_GroupsMovimientosByFecha()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var ayer = hoy.AddDays(-1);
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([CrearMovimiento(hoy), CrearMovimiento(ayer), CrearMovimiento(hoy)]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Equal(2, sut.Movimientos.Count);
        Assert.Equal(hoy, sut.Movimientos[0].Fecha);
        Assert.Equal(ayer, sut.Movimientos[1].Fecha);
    }

    [Fact]
    public async Task CargarMovimientosAsync_PassesFiltrosWithCurrentMonthRange()
    {
        var ahora = DateTime.Now;
        var primerDia = new DateOnly(ahora.Year, ahora.Month, 1);
        var ultimoDia = new DateOnly(ahora.Year, ahora.Month, DateTime.DaysInMonth(ahora.Year, ahora.Month));
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        await _movimientosService.Received(1).GetMovimientosAsync(
            IdCuenta,
            Arg.Is<GetMovimientosFilters?>(f =>
                f != null && f.FechaDesde == primerDia && f.FechaHasta == ultimoDia));
    }

    #endregion

    #region Navegación de meses

    [Fact]
    public async Task AnteriorCommand_DecrementsFechaByOneMonth()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([]);

        var sut = CreateSut();
        var fechaInicial = sut.Fecha;
        await sut.AnteriorCommand.ExecuteAsync(null);

        Assert.Equal(fechaInicial.AddMonths(-1).Month, sut.Fecha.Month);
    }

    [Fact]
    public async Task SiguienteCommand_IncrementsFechaByOneMonth()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([]);

        var sut = CreateSut();
        sut.Fecha = DateTime.Now.AddMonths(-2);
        await sut.SiguienteCommand.ExecuteAsync(null);

        Assert.Equal(DateTime.Now.AddMonths(-1).Month, sut.Fecha.Month);
    }

    [Fact]
    public async Task AnteriorCommand_ReloadsMovimientos()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([]);

        var sut = CreateSut();
        await sut.AnteriorCommand.ExecuteAsync(null);

        await _movimientosService.Received().GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>());
    }

    #endregion

    #region Propiedades calculadas

    [Fact]
    public void Mes_ReturnsFormattedCurrentMonth()
    {
        var sut = CreateSut();
        Assert.Equal(sut.Fecha.ToString("MMMM, yyyy"), sut.Mes);
    }

    [Fact]
    public void MesAnterior_ReturnsPreviousMonthName()
    {
        var sut = CreateSut();
        Assert.Equal(sut.Fecha.AddMonths(-1).ToString("MMMM"), sut.MesAnterior);
    }

    [Fact]
    public void MesSiguiente_ReturnsNextMonthName()
    {
        var sut = CreateSut();
        sut.Fecha = DateTime.Now.AddMonths(-1);
        Assert.Equal(sut.Fecha.AddMonths(1).ToString("MMMM"), sut.MesSiguiente);
    }

    [Fact]
    public void IsSiguienteEnabled_WhenFechaIsCurrentMonth_IsFalse()
    {
        var sut = CreateSut();
        sut.Fecha = DateTime.Now;
        Assert.False(sut.IsSiguienteEnabled);
    }

    [Fact]
    public void IsSiguienteEnabled_WhenFechaIsBeforeCurrentMonth_IsTrue()
    {
        var sut = CreateSut();
        sut.Fecha = DateTime.Now.AddMonths(-1);
        Assert.True(sut.IsSiguienteEnabled);
    }

    #endregion

    #region NuevoMovimientoCommand

    [Fact]
    public async Task NuevoMovimientoCommand_WhenCuentaLoaded_NavigatesToCrearMovimientoWithCorrectId()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();
        await sut.NuevoMovimientoCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync($"crear-movimiento?idCuenta={IdCuenta}");
    }

    [Fact]
    public async Task NuevoMovimientoCommand_WhenNoCuentaLoaded_DoesNotNavigate()
    {
        var sut = CreateSut();

        await sut.NuevoMovimientoCommand.ExecuteAsync(null);

        await _navigationService.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    #endregion

    #region EliminarMovimientoCommand

    [Fact]
    public async Task EliminarMovimientoCommand_WhenExecuted_CallsDeleteService()
    {
        var movimiento = CrearMovimiento(DateOnly.FromDateTime(DateTime.Now));
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([movimiento]);
        _confirmationService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();
        await sut.EliminarMovimientoCommand.ExecuteAsync(movimiento);

        await _movimientosService.Received(1).DeleteMovimientoAsync(movimiento.IdCuenta, movimiento.IdMovimiento);
    }

    [Fact]
    public async Task EliminarMovimientoCommand_WhenSuccessful_RecargarMovimientos()
    {
        var movimiento = CrearMovimiento(DateOnly.FromDateTime(DateTime.Now));
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([movimiento]);
        _confirmationService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();
        await sut.EliminarMovimientoCommand.ExecuteAsync(movimiento);

        await _movimientosService.Received(2).GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>());
    }

    [Fact]
    public async Task EliminarMovimientoCommand_WhenFails_SetsError()
    {
        var movimiento = CrearMovimiento(DateOnly.FromDateTime(DateTime.Now));
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>()).Returns([movimiento]);
        _confirmationService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _movimientosService.DeleteMovimientoAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Throws<Exception>();

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();
        await sut.EliminarMovimientoCommand.ExecuteAsync(movimiento);

        Assert.Equal("No se pudo eliminar el movimiento. Inténtalo de nuevo.", sut.Error);
    }

    #endregion
}
