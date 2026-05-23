using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class MovimientosViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();
    private readonly IMovimientosService _movimientosService = Substitute.For<IMovimientosService>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();

    private MovimientosViewModel CreateSut() => new(_cuentasService, _movimientosService, _navigationService);

    private void ConfigurarCuenta(string descripcion = "Mi cuenta")
        => _cuentasService.GetDefaultCuentaAsync().Returns((IdCuenta, (string?)descripcion));

    private void ConfigurarSinCuenta()
        => _cuentasService.GetDefaultCuentaAsync().Returns(((Guid?)null, (string?)null));

    private static MovimientoItem CrearMovimiento(DateOnly fecha, decimal importe = 10m) => new()
    {
        IdMovimiento = Guid.NewGuid(),
        IdCuenta = IdCuenta,
        Concepto = "Test",
        Importe = importe,
        Moneda = "EUR",
        TipoMovimiento = TipoMovimiento.Gasto,
        FechaMovimiento = fecha
    };

    // ── Estado inicial ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_IsBusyIsFalse()
        => Assert.False(CreateSut().IsBusy);

    [Fact]
    public void Constructor_MovimientosIsEmpty()
        => Assert.Empty(CreateSut().Movimientos);

    [Fact]
    public void Constructor_ErrorIsEmpty()
        => Assert.Equal(string.Empty, CreateSut().Error);

    [Fact]
    public void Constructor_NombreCuentaIsEmpty()
        => Assert.Equal(string.Empty, CreateSut().NombreCuenta);

    [Fact]
    public void Constructor_FechaIsCurrentMonth()
    {
        var now = DateTime.Now;
        var sut = CreateSut();
        Assert.Equal(now.Year, sut.Fecha.Year);
        Assert.Equal(now.Month, sut.Fecha.Month);
    }

    // ── SinMovimientos ────────────────────────────────────────────────────────

    [Fact]
    public void SinMovimientos_WhenCollectionIsEmpty_IsTrue()
        => Assert.True(CreateSut().SinMovimientos);

    [Fact]
    public async Task SinMovimientos_WhenMovimientosLoaded_IsFalse()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([CrearMovimiento(DateOnly.FromDateTime(DateTime.Now))]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.False(sut.SinMovimientos);
    }

    [Fact]
    public async Task SinMovimientos_WhenHasError_IsFalse()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.False(sut.SinMovimientos);
    }

    // ── HasMovimientos ────────────────────────────────────────────────────────

    [Fact]
    public async Task HasMovimientos_WhenMovimientosLoaded_IsTrue()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([CrearMovimiento(DateOnly.FromDateTime(DateTime.Now))]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.True(sut.HasMovimientos);
    }

    [Fact]
    public async Task HasMovimientos_WhenHasError_IsFalse()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.False(sut.HasMovimientos);
    }

    // ── HasError ──────────────────────────────────────────────────────────────

    [Fact]
    public void HasError_InitiallyIsFalse()
        => Assert.False(CreateSut().HasError);

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

    // ── CargarMovimientosAsync ────────────────────────────────────────────────

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

        Assert.NotEmpty(sut.Movimientos);
        Assert.Equal(2, sut.Movimientos.Sum(g => g.Count()));
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
    public async Task CargarMovimientosAsync_WhenSuccessful_IsBusyIsFalseAfterCompletion()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task CargarMovimientosAsync_WhenServiceThrows_SetsError()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("Error de red"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.Equal("Error de red", sut.Error);
        Assert.True(sut.HasError);
    }

    [Fact]
    public async Task CargarMovimientosAsync_WhenServiceThrows_IsBusyIsFalseAfterCompletion()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("Error de red"));

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        Assert.False(sut.IsBusy);
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
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        await _movimientosService.Received(1).GetMovimientosAsync(
            IdCuenta,
            Arg.Is<GetMovimientosFilters?>(f =>
                f != null && f.FechaDesde == primerDia && f.FechaHasta == ultimoDia));
    }

    [Fact]
    public async Task CargarMovimientosAsync_MakesOnlyOneCuentasCall()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();

        await _cuentasService.Received(1).GetDefaultCuentaAsync();
    }

    // ── Navegación de meses ───────────────────────────────────────────────────

    [Fact]
    public async Task AnteriorCommand_DecrementsFechaByOneMonth()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        var fechaInicial = sut.Fecha;
        await sut.AnteriorCommand.ExecuteAsync(null);

        Assert.Equal(fechaInicial.AddMonths(-1).Month, sut.Fecha.Month);
    }

    [Fact]
    public async Task SiguienteCommand_IncrementsFechaByOneMonth()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        sut.Fecha = DateTime.Now.AddMonths(-2);
        await sut.SiguienteCommand.ExecuteAsync(null);

        Assert.Equal(DateTime.Now.AddMonths(-1).Month, sut.Fecha.Month);
    }

    [Fact]
    public async Task AnteriorCommand_ReloadsMovimientos()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        await sut.AnteriorCommand.ExecuteAsync(null);

        await _movimientosService.Received().GetMovimientosAsync(
            IdCuenta, Arg.Any<GetMovimientosFilters?>());
    }

    // ── Propiedades calculadas ────────────────────────────────────────────────

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

    // ── NuevoMovimientoCommand ────────────────────────────────────────────────

    [Fact]
    public async Task NuevoMovimientoCommand_WhenCuentaLoaded_NavigatesToCrearMovimiento()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();
        await sut.NuevoMovimientoCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync(
            Arg.Is<string>(s => s.StartsWith("//crear-movimiento") && s.Contains(IdCuenta.ToString())));
    }

    [Fact]
    public async Task NuevoMovimientoCommand_WhenNoCuentaLoaded_DoesNotNavigate()
    {
        var sut = CreateSut();

        await sut.NuevoMovimientoCommand.ExecuteAsync(null);

        await _navigationService.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task NuevoMovimientoCommand_WhenCuentaLoaded_PassesCorrectIdCuenta()
    {
        ConfigurarCuenta();
        _movimientosService.GetMovimientosAsync(IdCuenta, Arg.Any<GetMovimientosFilters?>())
            .Returns([]);

        var sut = CreateSut();
        await sut.CargarMovimientosAsync();
        await sut.NuevoMovimientoCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync(
            $"//crear-movimiento?idCuenta={IdCuenta}");
    }
}
