using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Features.Movimientos;
using HA.TFG.AppFinanzas.Core.Features.Movimientos.Dtos;
using HA.TFG.AppFinanzas.Core.Features.Movimientos.Models;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.Tests.Fixtures;
using HA.TFG.AppFinanzas.Core.Utilities;
using HA.TFG.AppFinanzas.Core.Features.Movimientos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.Core.Tests.ViewModels;

public class MovimientoViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();
    private readonly IMovimientosService _movimientosService = Substitute.For<IMovimientosService>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly IComprobantePickerService _comprobantePickerService = Substitute.For<IComprobantePickerService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();

    private static readonly CategoriaItem CategoriaDefault = TestDataBuilder.Categoria
        .WithName("Alimentación")
        .Build();

    private MovimientoViewModel CreateSut() =>
        new(_cuentasService, _movimientosService, _navigationService, _comprobantePickerService);

    private MovimientoViewModel CreateSutWithValidForm()
    {
        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = CategoriaDefault;
        return sut;
    }

    #region Estado inicial

    [Fact]
    public void Constructor_InitialState_MatchesExpectedSnapshot()
    {
        var sut = CreateSut();

        Assert.Multiple(
            () => Assert.Equal(string.Empty, sut.Concepto),
            () => Assert.Equal(string.Empty, sut.ImporteTexto),
            () => Assert.Equal(TipoMovimiento.Gasto, sut.TipoSeleccionado),
            () => Assert.Equal(DateTime.Today, sut.Fecha),
            () => Assert.Equal(string.Empty, sut.Error),
            () => Assert.False(sut.IsBusy),
            () => Assert.Null(sut.CategoriaSeleccionada),
            () => Assert.Equal(MonedasHelper.DefaultMoneda, sut.MonedaSeleccionada),
            () => Assert.Equal([TipoMovimiento.Gasto, TipoMovimiento.Ingreso, TipoMovimiento.Transferencia], sut.Tipos));
    }

    #endregion

    #region Reset

    [Fact]
    public void Reset_ClearsAllFields()
    {
        var sut = CreateSut();
        sut.Concepto = "Taxi";
        sut.ImporteTexto = "15.50";
        sut.TipoSeleccionado = TipoMovimiento.Ingreso;
        sut.Fecha = DateTime.Today.AddDays(-3);
        sut.Error = "Algún error";
        sut.CategoriaSeleccionada = CategoriaDefault;

        sut.Reset();

        Assert.Multiple(
            () => Assert.Equal(string.Empty, sut.Concepto),
            () => Assert.Equal(string.Empty, sut.ImporteTexto),
            () => Assert.Equal(TipoMovimiento.Gasto, sut.TipoSeleccionado),
            () => Assert.Equal(DateTime.Today, sut.Fecha),
            () => Assert.Equal(string.Empty, sut.Error),
            () => Assert.Null(sut.CategoriaSeleccionada),
            () => Assert.Equal(MonedasHelper.DefaultMoneda, sut.MonedaSeleccionada));
    }

    #endregion

    #region CategoriasFiltradas

    [Fact]
    public async Task IdCuenta_WhenSet_CargaCategoriasDesdeServicio()
    {
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns([]);

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        await sut.CargandoCategoriasTask;

        await _cuentasService.Received(1).GetCategoriasAsync(IdCuenta);
    }

    [Fact]
    public async Task CategoriasFiltradas_FiltraPorTipoSeleccionado()
    {
        var gastoId = Guid.NewGuid();
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(
        [
            new() { IdCuentaCategoria = gastoId,        Nombre = "Alimentación", TipoMovimiento = TipoMovimiento.Gasto },
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Nómina",       TipoMovimiento = TipoMovimiento.Ingreso }
        ]);

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        await sut.CargandoCategoriasTask;
        sut.TipoSeleccionado = TipoMovimiento.Gasto;

        Assert.Equal([gastoId], sut.CategoriasFiltradas.Select(c => c.IdCuentaCategoria));
    }

    [Fact]
    public async Task CategoriasFiltradas_WhenTipoCambia_ActualizaLista()
    {
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(
        [
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Alimentación", TipoMovimiento = TipoMovimiento.Gasto },
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Nómina",       TipoMovimiento = TipoMovimiento.Ingreso }
        ]);

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        await sut.CargandoCategoriasTask;
        sut.TipoSeleccionado = TipoMovimiento.Ingreso;

        Assert.Equal(["Nómina"], sut.CategoriasFiltradas.Select(c => c.Nombre));
    }

    #endregion

    #region CargarMovimientoAsync

    [Fact]
    public async Task CargarMovimientoAsync_WhenSuccessful_EntersEditModeAndPopulatesFields()
    {
        var categoriaId = Guid.NewGuid();
        var fechaMovimiento = new DateTime(2025, 6, 15, 14, 30, 0);

        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(
        [
            new() { IdCuentaCategoria = categoriaId, Nombre = "Alimentación", TipoMovimiento = TipoMovimiento.Gasto }
        ]);
        _movimientosService.GetMovimientoDetalleAsync(IdCuenta, Arg.Any<Guid>()).Returns(new MovimientoDetalleItem
        {
            IdCuenta = IdCuenta,
            IdCuentaCategoria = categoriaId,
            TipoMovimiento = TipoMovimiento.Gasto,
            Concepto = "Supermercado",
            Establecimiento = "Mercadona",
            Importe = 42.50m,
            Moneda = "EUR",
            Nota = "Compra semanal",
            FechaMovimiento = fechaMovimiento
        });

        var sut = CreateSut();
        await sut.CargarMovimientoAsync(IdCuenta, Guid.NewGuid());

        Assert.Multiple(
            () => Assert.True(sut.ModoEdicion),
            () => Assert.Equal("Editar movimiento", sut.TituloFormulario),
            () => Assert.Equal("Guardar cambios", sut.GuardarMovimientoText),
            () => Assert.Equal("Supermercado", sut.Concepto),
            () => Assert.Equal("Mercadona", sut.Establecimiento),
            () => Assert.Equal("Compra semanal", sut.Nota),
            () => Assert.Equal("42.50", sut.ImporteTexto),
            () => Assert.Equal(TipoMovimiento.Gasto, sut.TipoSeleccionado),
            () => Assert.Equal(fechaMovimiento.Date, sut.Fecha),
            () => Assert.Equal(fechaMovimiento.TimeOfDay, sut.Hora),
            () => Assert.Equal(categoriaId, sut.CategoriaSeleccionada?.IdCuentaCategoria),
            () => Assert.Null(sut.Comprobante));
    }

    [Fact]
    public async Task CargarMovimientoAsync_WhenServiceThrows_SetsErrorAndStopsLoading()
    {
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns([]);
        _movimientosService.GetMovimientoDetalleAsync(IdCuenta, Arg.Any<Guid>())
            .ThrowsAsyncForAnyArgs(new HttpRequestException("Error de red"));

        var sut = CreateSut();
        await sut.CargarMovimientoAsync(IdCuenta, Guid.NewGuid());

        Assert.Multiple(
            () => Assert.Equal("No se pudo cargar el movimiento. Inténtalo de nuevo.", sut.Error),
            () => Assert.False(sut.IsBusy));
    }

    #endregion

    #region GuardarMovimientoCommand — crear

    [Fact]
    public async Task GuardarMovimientoAsync_WhenSuccessful_CallsServiceAndNavigatesBack()
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
                IdCuenta, "Supermercado", 42.50m,
                sut.MonedaSeleccionada.Key, TipoMovimiento.Gasto,
                new DateTime(2025, 6, 15, 14, 30, 0),
                CategoriaDefault.IdCuentaCategoria),
            Arg.Any<CancellationToken>());
        await _navigationService.Received(1).GoBackAsync();
    }

    [Fact]
    public async Task GuardarMovimientoAsync_WhenImporteIsInvalid_SetsErrorAndDoesNotCallService()
    {
        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "abc";
        sut.CategoriaSeleccionada = CategoriaDefault;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        Assert.Equal("El importe debe ser un número mayor que cero.", sut.Error);
        await _movimientosService.DidNotReceiveWithAnyArgs().CreateMovimientoAsync(default!);
        await _movimientosService.DidNotReceiveWithAnyArgs().UpdateMovimientoAsync(default!);
    }

    [Fact]
    public async Task GuardarMovimientoAsync_WhenServiceThrows_SetsErrorAndDoesNotNavigate()
    {
        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ThrowsAsyncForAnyArgs(new HttpRequestException("Error de red"));

        var sut = CreateSutWithValidForm();
        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.NotEmpty(sut.Error),
            () => Assert.False(sut.IsBusy));
        await _navigationService.DidNotReceive().GoBackAsync();
    }

    [Fact]
    public async Task GuardarMovimientoAsync_WhenCalledAfterError_ClearsErrorBeforeAttempt()
    {
        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ThrowsAsyncForAnyArgs(new HttpRequestException("Error"));

        var sut = CreateSutWithValidForm();
        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        _movimientosService
            .CreateMovimientoAsync(Arg.Any<CreateMovimientoDto>())
            .ReturnsForAnyArgs(Task.CompletedTask);
        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, sut.Error);
    }

    #endregion

    #region GuardarMovimientoCommand — editar

    [Fact]
    public async Task GuardarMovimientoAsync_WhenEditSuccessful_CallsUpdateAndNavigatesBack()
    {
        var idMovimiento = Guid.NewGuid();
        var categoria = new CategoriaItem
        {
            IdCuentaCategoria = Guid.NewGuid(),
            Nombre = "Alimentación",
            TipoMovimiento = TipoMovimiento.Gasto
        };

        _cuentasService.GetCategoriasAsync(IdCuenta).Returns([categoria]);
        _movimientosService.GetMovimientoDetalleAsync(IdCuenta, idMovimiento).Returns(new MovimientoDetalleItem
        {
            IdCuenta = IdCuenta,
            IdCuentaCategoria = categoria.IdCuentaCategoria,
            TipoMovimiento = TipoMovimiento.Gasto,
            Concepto = "Anterior",
            Importe = 10m,
            Moneda = "EUR",
            FechaMovimiento = DateTime.Today
        });

        var sut = CreateSut();
        await sut.CargarMovimientoAsync(IdCuenta, idMovimiento);
        sut.Concepto = "Supermercado";
        sut.ImporteTexto = "42.50";
        sut.CategoriaSeleccionada = categoria;

        await sut.GuardarMovimientoCommand.ExecuteAsync(null);

        await _movimientosService.Received(1).UpdateMovimientoAsync(
            Arg.Is<UpdateMovimientoDto>(d => d.IdCuenta == IdCuenta && d.IdMovimiento == idMovimiento),
            Arg.Any<CancellationToken>());
        await _navigationService.Received(1).GoBackAsync();
    }

    #endregion

    #region Comprobante

    [Fact]
    public async Task OpenComprobanteOptionsCommand_WhenUserSelectsFile_SetsComprobante()
    {
        var comprobante = new ComprobanteResult([1, 2, 3], "ticket.jpg", "image/jpeg");
        _navigationService
            .DisplayActionSheetAsync("Adjuntar comprobante", "Cancelar", null, "Seleccionar archivo", "Tomar foto")
            .Returns("Seleccionar archivo");
        _comprobantePickerService.SeleccionarArchivoAsync(Arg.Any<CancellationToken>()).Returns(comprobante);

        var sut = CreateSut();
        await sut.OpenComprobanteOptionsCommand.ExecuteAsync(null);

        Assert.Equal(comprobante, sut.Comprobante);
    }

    [Fact]
    public async Task AttachFileCommand_WhenSuccessful_SetsComprobante()
    {
        var comprobante = new ComprobanteResult([1, 2, 3], "ticket.jpg", "image/jpeg");
        _comprobantePickerService.SeleccionarArchivoAsync(Arg.Any<CancellationToken>()).Returns(comprobante);

        var sut = CreateSut();
        await sut.AttachFileCommand.ExecuteAsync(null);

        Assert.Equal(comprobante, sut.Comprobante);
    }

    [Fact]
    public async Task TakePhotoCommand_WhenSuccessful_SetsComprobante()
    {
        var comprobante = new ComprobanteResult([1, 2, 3], "foto.jpg", "image/jpeg");
        _comprobantePickerService.TomarFotoAsync(Arg.Any<CancellationToken>()).Returns(comprobante);

        var sut = CreateSut();
        await sut.TakePhotoCommand.ExecuteAsync(null);

        Assert.Equal(comprobante, sut.Comprobante);
    }

    [Fact]
    public void RemoveComprobanteCommand_ClearsComprobante()
    {
        var sut = CreateSut();
        sut.Comprobante = new ComprobanteResult([1], "ticket.jpg", "image/jpeg");

        sut.RemoveComprobanteCommand.Execute(null);

        Assert.Null(sut.Comprobante);
    }

    [Fact]
    public async Task ScanComprobanteCommand_WhenSuccessful_FillsFieldsAndSetsComprobante()
    {
        var comprobante = new ComprobanteResult([1, 2, 3], "ticket.jpg", "image/jpeg");
        var categoriaId = Guid.NewGuid();

        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(
        [
            new() { IdCuentaCategoria = categoriaId, Nombre = "Alimentación", TipoMovimiento = TipoMovimiento.Gasto }
        ]);
        _navigationService
            .DisplayActionSheetAsync("Adjuntar comprobante", "Cancelar", null, "Seleccionar archivo", "Tomar foto")
            .Returns("Seleccionar archivo");
        _comprobantePickerService.SeleccionarArchivoAsync(Arg.Any<CancellationToken>()).Returns(comprobante);
        _movimientosService.EscanearComprobanteAsync(IdCuenta, comprobante, Arg.Any<CancellationToken>())
            .Returns(new ComprobanteExtraidoDto
            {
                Concepto = "Cafetería",
                Establecimiento = "Starbucks",
                Importe = 12.30m,
                Moneda = "EUR",
                FechaMovimiento = new DateTimeOffset(2025, 6, 15, 10, 15, 0, TimeSpan.Zero),
                TipoMovimiento = "Gasto",
                IdCuentaCategoria = categoriaId,
                Nota = "Desayuno"
            });

        var sut = CreateSut();
        sut.IdCuenta = IdCuenta;
        await sut.CargandoCategoriasTask;

        await sut.ScanComprobanteCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.Equal(comprobante, sut.Comprobante),
            () => Assert.Equal("Cafetería", sut.Concepto),
            () => Assert.Equal("Starbucks", sut.Establecimiento),
            () => Assert.Equal("Desayuno", sut.Nota),
            () => Assert.Equal("12.30", sut.ImporteTexto),
            () => Assert.Equal(TipoMovimiento.Gasto, sut.TipoSeleccionado),
            () => Assert.Equal(categoriaId, sut.CategoriaSeleccionada?.IdCuentaCategoria),
            () => Assert.False(sut.IsBusy));
    }

    #endregion

    #region CancelCommand

    [Fact]
    public async Task CancelCommand_NavigatesBack()
    {
        var sut = CreateSut();

        await sut.CancelCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoBackAsync();
    }

    #endregion
}
