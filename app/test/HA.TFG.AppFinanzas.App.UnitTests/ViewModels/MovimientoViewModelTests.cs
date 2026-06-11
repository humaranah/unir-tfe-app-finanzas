using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.Utilities;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class MovimientoViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();
    private readonly IMovimientosService _movimientosService = Substitute.For<IMovimientosService>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly IComprobantePickerService _comprobantePickerService = Substitute.For<IComprobantePickerService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();

    private static readonly CategoriaItem CategoriaDefault = new()
    {
        IdCuentaCategoria = Guid.NewGuid(),
        Nombre = "Alimentación",
        TipoMovimiento = TipoMovimiento.Gasto
    };

    private MovimientoViewModel CreateSut() =>
        new(_cuentasService, _movimientosService, _navigationService, _comprobantePickerService);

    // ── Estado inicial ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ConceptoIsEmpty()
        => Assert.Equal(string.Empty, CreateSut().Concepto);

    [Fact]
    public void Constructor_ImporteTextoIsEmpty()
        => Assert.Equal(string.Empty, CreateSut().ImporteTexto);

    [Fact]
    public void Constructor_TipoSeleccionadoIsGasto()
        => Assert.Equal(TipoMovimiento.Gasto, CreateSut().TipoSeleccionado);

    [Fact]
    public void Constructor_FechaIsToday()
        => Assert.Equal(DateTime.Today, CreateSut().Fecha);

    [Fact]
    public void Constructor_ErrorIsEmpty()
        => Assert.Equal(string.Empty, CreateSut().Error);

    [Fact]
    public void Constructor_IsBusyIsFalse()
        => Assert.False(CreateSut().IsBusy);

    [Fact]
    public void Constructor_CategoriaSeleccionadaIsNull()
        => Assert.Null(CreateSut().CategoriaSeleccionada);

    [Fact]
    public void Constructor_MonedaSeleccionadaIsDefault()
        => Assert.Equal(MonedasHelper.DefaultMoneda, CreateSut().MonedaSeleccionada);

    [Fact]
    public void Constructor_TiposContainsExpectedValues()
    {
        var sut = CreateSut();
        Assert.Contains(TipoMovimiento.Gasto, sut.Tipos);
        Assert.Contains(TipoMovimiento.Ingreso, sut.Tipos);
        Assert.Contains(TipoMovimiento.Transferencia, sut.Tipos);
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ClearsAllFields()
    {
        var sut = CreateSut();
        sut.Concepto = "Taxi";
        sut.ImporteTexto = "15.50";
        sut.TipoSeleccionado = TipoMovimiento.Ingreso;
        sut.Fecha = DateTime.Today.AddDays(-3);
        sut.Error = "Algún error";

        sut.Reset();

        Assert.Equal(string.Empty, sut.Concepto);
        Assert.Equal(string.Empty, sut.ImporteTexto);
        Assert.Equal(TipoMovimiento.Gasto, sut.TipoSeleccionado);
        Assert.Equal(DateTime.Today, sut.Fecha);
        Assert.Equal(string.Empty, sut.Error);
        Assert.Null(sut.CategoriaSeleccionada);
        Assert.Equal(MonedasHelper.DefaultMoneda, sut.MonedaSeleccionada);
    }

    // ── CategoriasFiltradas ───────────────────────────────────────────────────

    [Fact]
    public async Task IdCuenta_WhenSet_CargaCategoriasDesdeServicio()
    {
        var categorias = new List<CategoriaItem>
        {
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Alimentación", TipoMovimiento = TipoMovimiento.Gasto },
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Nómina",       TipoMovimiento = TipoMovimiento.Ingreso }
        };
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(categorias);

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;

        await sut.CargandoCategoriasTask;

        await _cuentasService.Received(1).GetCategoriasAsync(IdCuenta);
    }

    [Fact]
    public async Task CategoriasFiltradas_FiltraPorTipoSeleccionado()
    {
        var gastoId = Guid.NewGuid();
        var categorias = new List<CategoriaItem>
        {
            new() { IdCuentaCategoria = gastoId,       Nombre = "Alimentación", TipoMovimiento = TipoMovimiento.Gasto },
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Nómina",       TipoMovimiento = TipoMovimiento.Ingreso }
        };
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(categorias);

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        await sut.CargandoCategoriasTask;

        sut.TipoSeleccionado = TipoMovimiento.Gasto;
        var filtradas = sut.CategoriasFiltradas;

        Assert.Single(filtradas);
        Assert.Equal(gastoId, filtradas[0].IdCuentaCategoria);
    }

    [Fact]
    public async Task CategoriasFiltradas_WhenTipoCambia_ActualizaLista()
    {
        var categorias = new List<CategoriaItem>
        {
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Alimentación", TipoMovimiento = TipoMovimiento.Gasto },
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Nómina",       TipoMovimiento = TipoMovimiento.Ingreso }
        };
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(categorias);

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        await sut.CargandoCategoriasTask;

        sut.TipoSeleccionado = TipoMovimiento.Ingreso;

        Assert.Single(sut.CategoriasFiltradas);
        Assert.Equal("Nómina", sut.CategoriasFiltradas[0].Nombre);
    }

    // ── GuardarMovimientoCommand ────────────────────────────────────────────────

    [Fact]
    public async Task CrearMovimientoAsync_WhenSuccessful_CallsServiceWithCorrectParameters()
    {
        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.TipoSeleccionado = TipoMovimiento.Gasto;
        sut.Fecha = new DateTime(2025, 6, 15);
        sut.Hora = new TimeSpan(14, 30, 0);
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        await _movimientosService.Received(1).CreateMovimientoAsync(
            new CreateMovimientoDto(
                IdCuenta,
                "Supermercado",
                42.50m,
                sut.MonedaSeleccionada.Key,
                TipoMovimiento.Gasto,
                new DateTime(2025, 6, 15, 14, 30, 0),
                CategoriaDefault.IdCuentaCategoria),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenSuccessful_NavigatesToMovimientos()
    {
        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync("//movimientos");
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenSuccessful_IsBusyIsFalseAfterCompletion()
    {
        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenSuccessful_ErrorRemainsEmpty()
    {
        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, sut.Error);
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenSuccessful_PassesCategoriaIdWhenSelected()
    {
        var categoria = new CategoriaItem
        {
            IdCuentaCategoria = Guid.NewGuid(),
            Nombre = "Alimentación",
            TipoMovimiento = TipoMovimiento.Gasto
        };

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Mercadona";
        sut.ImporteTexto = "30";
        sut.CategoriaSeleccionada = categoria;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        await _movimientosService.Received(1).CreateMovimientoAsync(
            Arg.Is<CreateMovimientoDto>(d => d.IdCuentaCategoria == categoria.IdCuentaCategoria),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenServiceThrows_SetsError()
    {
        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ThrowsAsyncForAnyArgs(new HttpRequestException("Error de red"));

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        Assert.True(sut.HasError);
        Assert.NotEmpty(sut.Error);
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenServiceThrows_IsBusyIsFalseAfterCompletion()
    {
        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ThrowsAsyncForAnyArgs(new HttpRequestException("Error de red"));

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenServiceThrows_DoesNotNavigate()
    {
        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ThrowsAsyncForAnyArgs(new HttpRequestException("Error de red"));

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        await _navigationService.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CrearMovimientoAsync_WhenCalledAfterError_ClearsErrorBeforeAttempt()
    {
        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ThrowsAsyncForAnyArgs(new HttpRequestException("Error"));

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null); // primer intento fallido

        _movimientosService.ClearReceivedCalls();
        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        await sut.GuardarMovimientoCommand.ExecuteAsync(null); // segundo intento exitoso

        Assert.Equal(string.Empty, sut.Error);
    }

    // ── CancelarCommand ───────────────────────────────────────────────────────

    [Fact]
    public async Task CancelCommand_NavigatesToMovimientos()
    {
        var sut = CreateSut();

        await sut.CancelCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync("//movimientos");
    }
}
