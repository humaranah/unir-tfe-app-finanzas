using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.Services;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Globalization;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class MovimientoDetalleViewModelTests
{
    private readonly IMovimientosService _movimientosService = Substitute.For<IMovimientosService>();
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly IComprobanteViewerService _comprobanteViewerService = Substitute.For<IComprobanteViewerService>();
    private readonly IConfirmationService _confirmationService = Substitute.For<IConfirmationService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();
    private static readonly Guid IdMovimiento = Guid.NewGuid();
    private static readonly Guid IdCategoria = Guid.NewGuid();

    private static readonly CategoriaItem CategoriaDefault = new()
    {
        IdCuentaCategoria = IdCategoria,
        Nombre = "Alimentación",
        TipoMovimiento = TipoMovimiento.Gasto
    };

    private static readonly MovimientoDetalleItem DetalleDefault = new()
    {
        IdMovimiento = IdMovimiento,
        IdCuenta = IdCuenta,
        IdCuentaCategoria = IdCategoria,
        TipoMovimiento = TipoMovimiento.Gasto,
        Concepto = "Supermercado",
        Establecimiento = "Mercadona",
        Importe = 42.50m,
        Moneda = "EUR",
        Nota = "Compra semanal",
        FechaMovimiento = new DateTime(2025, 6, 1, 14, 30, 0)
    };

    private MovimientoDetalleViewModel CreateSut() =>
        new(_movimientosService, _cuentasService, _navigationService, _comprobanteViewerService, _confirmationService);

    private void ConfigurarServiciosOk(MovimientoDetalleItem? detalle = null)
    {
        _movimientosService
            .GetMovimientoDetalleAsync(IdCuenta, IdMovimiento)
            .Returns(detalle ?? DetalleDefault);
        _cuentasService
            .GetCategoriasAsync(IdCuenta)
            .Returns([CategoriaDefault]);
    }

    // ── Estado inicial ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_InitialState_MatchesExpectedSnapshot()
    {
        var sut = CreateSut();

        Assert.Multiple(
            () => Assert.Equal(string.Empty, sut.Concepto),
            () => Assert.Equal(string.Empty, sut.Moneda),
            () => Assert.Equal(0m, sut.Importe),
            () => Assert.Equal(string.Empty, sut.Error),
            () => Assert.False(sut.IsBusy),
            () => Assert.False(sut.HasError),
            () => Assert.True(sut.IsNotBusy),
            () => Assert.False(sut.HasDetalle),
            () => Assert.False(sut.TieneEstablecimiento),
            () => Assert.False(sut.TieneNota),
            () => Assert.False(sut.TieneComprobante),
            () => Assert.False(sut.IsBusyComprobante),
            () => Assert.True(sut.IsNotBusyComprobante)
        );
    }

    // ── CargarDetalleAsync – happy path ──────────────────────────────────────

    [Fact]
    public async Task CargarDetalleAsync_CuandoExito_PopulaTodasLasPropiedades()
    {
        ConfigurarServiciosOk();
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.Multiple(
            () => Assert.Equal("Supermercado", sut.Concepto),
            () => Assert.Equal("Mercadona", sut.Establecimiento),
            () => Assert.Equal("Compra semanal", sut.Nota),
            () => Assert.Equal(42.50m, sut.Importe),
            () => Assert.Equal("EUR", sut.Moneda),
            () => Assert.Equal(TipoMovimiento.Gasto, sut.Tipo),
            () => Assert.Equal(new DateTime(2025, 6, 1, 14, 30, 0), sut.FechaMovimiento),
            () => Assert.Equal("Alimentación", sut.NombreCategoria),
            () => Assert.True(sut.TieneEstablecimiento),
            () => Assert.True(sut.TieneNota),
            () => Assert.False(sut.TieneComprobante),
            () => Assert.Equal(string.Empty, sut.Error),
            () => Assert.False(sut.IsBusy),
            () => Assert.True(sut.HasDetalle)
        );
    }

    [Fact]
    public async Task CargarDetalleAsync_CuandoExito_LlamaServiciosConParametrosCorrectos()
    {
        ConfigurarServiciosOk();
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        await _movimientosService.Received(1).GetMovimientoDetalleAsync(IdCuenta, IdMovimiento);
        await _cuentasService.Received(1).GetCategoriasAsync(IdCuenta);
    }

    [Fact]
    public async Task CargarDetalleAsync_CuandoSinEstablecimientoNiNota_TieneEstablecimientoYTieneNotaSonFalse()
    {
        var detalleMinimo = new MovimientoDetalleItem
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            IdCuentaCategoria = IdCategoria,
            TipoMovimiento = TipoMovimiento.Ingreso,
            Concepto = "Nómina",
            Establecimiento = null,
            Importe = 1500m,
            Moneda = "EUR",
            Nota = string.Empty,
            FechaMovimiento = DateTime.Today
        };
        ConfigurarServiciosOk(detalleMinimo);
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.Multiple(
            () => Assert.False(sut.TieneEstablecimiento),
            () => Assert.False(sut.TieneNota)
        );
    }

    // ── IsBusy ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CargarDetalleAsync_MientrasEjecuta_IsBusyEsTrue()
    {
        var tcs = new TaskCompletionSource<MovimientoDetalleItem>();
        _movimientosService
            .GetMovimientoDetalleAsync(IdCuenta, IdMovimiento)
            .Returns(tcs.Task);

        var sut = CreateSut();
        var cargandoTask = sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.True(sut.IsBusy);

        tcs.SetResult(DetalleDefault);
        await cargandoTask;
    }

    [Fact]
    public async Task CargarDetalleAsync_AlFinalizar_IsBusyEsFalse()
    {
        ConfigurarServiciosOk();
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.False(sut.IsBusy);
    }

    // ── Error ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CargarDetalleAsync_CuandoServicioLanzaExcepcion_SetError()
    {
        _movimientosService
            .GetMovimientoDetalleAsync(IdCuenta, IdMovimiento)
            .ThrowsAsync(new InvalidOperationException("Error de red"));
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.Multiple(
            () => Assert.True(sut.HasError),
            () => Assert.NotEmpty(sut.Error),
            () => Assert.False(sut.IsBusy),
            () => Assert.False(sut.HasDetalle)
        );
    }

    // ── ImporteFormateado ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(TipoMovimiento.Gasto, 10.0, "EUR")]
    [InlineData(TipoMovimiento.Ingreso, 100.0, "USD")]
    [InlineData(TipoMovimiento.Transferencia, 50.0, "EUR")]
    public async Task ImporteFormateado_SegunTipo_MuestraSignoCorrecto(
        TipoMovimiento tipo, double importeDouble, string moneda)
    {
        var importe = (decimal)importeDouble;
        var importeFormateado = importe.ToString("N2", CultureInfo.InvariantCulture);
        var esperado = tipo switch
        {
            TipoMovimiento.Gasto => $"-{importeFormateado} {moneda}",
            TipoMovimiento.Ingreso => $"+{importeFormateado} {moneda}",
            _ => $"{importeFormateado} {moneda}"
        };
        var detalle = new MovimientoDetalleItem
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            IdCuentaCategoria = IdCategoria,
            TipoMovimiento = tipo,
            Concepto = "Test",
            Importe = importe,
            Moneda = moneda,
            FechaMovimiento = DateTime.Today
        };
        _movimientosService
            .GetMovimientoDetalleAsync(IdCuenta, IdMovimiento)
            .Returns(detalle);
        _cuentasService
            .GetCategoriasAsync(IdCuenta)
            .Returns([]);

        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.Equal(esperado, sut.ImporteFormateado, StringComparer.InvariantCulture);
    }

    // ── MostrarHora ───────────────────────────────────────────────────────────

    [Fact]
    public async Task MostrarHora_CuandoHoraEsCero_EsFalse()
    {
        var detalle = DetalleDefault with { FechaMovimiento = DateTime.Today };
        ConfigurarServiciosOk(detalle);
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.False(sut.MostrarHora);
    }

    [Fact]
    public async Task MostrarHora_CuandoHoraTieneValor_EsTrue()
    {
        ConfigurarServiciosOk();
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.True(sut.MostrarHora);
    }

    // ── VerComprobante ────────────────────────────────────────────────────────

    [Fact]
    public async Task CargarDetalleAsync_CuandoMovimientoTieneComprobante_TieneComprobanteEsTrue()
    {
        var detalleConComprobante = DetalleDefault with { TieneComprobante = true };
        ConfigurarServiciosOk(detalleConComprobante);
        var sut = CreateSut();

        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        Assert.True(sut.TieneComprobante);
    }

    [Fact]
    public async Task VerComprobanteCommand_CuandoServicioDevuelveComprobante_LlamaViewerService()
    {
        var comprobante = new ComprobanteResult([1, 2, 3], "ticket.jpg", "image/jpeg");
        _movimientosService
            .GetComprobanteAsync(Arg.Is(IdCuenta), Arg.Is(IdMovimiento), Arg.Any<CancellationToken>())
            .Returns(comprobante);
        var detalleConComprobante = DetalleDefault with { TieneComprobante = true };
        ConfigurarServiciosOk(detalleConComprobante);
        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        await sut.VerComprobanteCommand.ExecuteAsync(CancellationToken.None);

        await _comprobanteViewerService.Received(1).AbrirComprobanteAsync(comprobante, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerComprobanteCommand_CuandoServicioDevuelveNull_NoLlamaViewerService()
    {
        _movimientosService
            .GetComprobanteAsync(IdCuenta, IdMovimiento)
            .Returns((ComprobanteResult?)null);
        ConfigurarServiciosOk(DetalleDefault with { TieneComprobante = true });
        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        await sut.VerComprobanteCommand.ExecuteAsync(CancellationToken.None);

        await _comprobanteViewerService.DidNotReceive().AbrirComprobanteAsync(Arg.Any<ComprobanteResult>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerComprobanteCommand_CuandoServicioLanzaExcepcion_SetError()
    {
        _movimientosService
            .GetComprobanteAsync(Arg.Is(IdCuenta), Arg.Is(IdMovimiento), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("fallo"));
        ConfigurarServiciosOk(DetalleDefault with { TieneComprobante = true });
        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        await sut.VerComprobanteCommand.ExecuteAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.NotEmpty(sut.Error),
            () => Assert.False(sut.IsBusyComprobante)
        );
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    [Fact]
    public async Task EditarMovimientoCommand_NavegaAEditarMovimientoConParametrosCorrectos()
    {
        ConfigurarServiciosOk();
        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        await sut.EditarMovimientoCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync(
            $"editar-movimiento?idCuenta={IdCuenta}&idMovimiento={IdMovimiento}");
    }

    [Fact]
    public async Task VolverCommand_LlamaGoBackAsync()
    {
        var sut = CreateSut();

        await sut.VolverCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoBackAsync();
    }

    // ── Eliminar movimiento ────────────────────────────────────────────────

    [Fact]
    public async Task EliminarMovimientoCommand_WhenExecuted_CallsDeleteService()
    {
        ConfigurarServiciosOk();
        _confirmationService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        await sut.EliminarMovimientoCommand.ExecuteAsync(null);

        await _movimientosService.Received(1).DeleteMovimientoAsync(IdCuenta, IdMovimiento);
    }

    [Fact]
    public async Task EliminarMovimientoCommand_WhenSuccessful_NavegaAtras()
    {
        ConfigurarServiciosOk();
        _confirmationService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);

        await sut.EliminarMovimientoCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoBackAsync();
    }

    [Fact]
    public async Task EliminarMovimientoCommand_WhenFails_SetsError()
    {
        ConfigurarServiciosOk();
        _confirmationService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _movimientosService.DeleteMovimientoAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Throws<Exception>();

        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);
        await sut.EliminarMovimientoCommand.ExecuteAsync(null);

        Assert.Equal("No se pudo eliminar el movimiento. Inténtalo de nuevo.", sut.Error);
    }

    [Fact]
    public async Task EliminarMovimientoCommand_WhenFails_SetsBusyFalse()
    {
        ConfigurarServiciosOk();
        _confirmationService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _movimientosService.DeleteMovimientoAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Throws<Exception>();

        var sut = CreateSut();
        await sut.CargarDetalleAsync(IdCuenta, IdMovimiento);
        await sut.EliminarMovimientoCommand.ExecuteAsync(null);

        Assert.False(sut.IsBusy);
    }
}

