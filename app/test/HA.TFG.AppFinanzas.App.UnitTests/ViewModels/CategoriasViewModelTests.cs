using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.App.UnitTests.ViewModels;

public class CategoriasViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();

    private CategoriasViewModel CreateSut() => new(_cuentasService);

    private void ConfigurarCuenta(string descripcion = "Mi cuenta")
        => _cuentasService.GetDefaultCuentaAsync().Returns((IdCuenta, (string?)descripcion));

    private void ConfigurarSinCuenta()
        => _cuentasService.GetDefaultCuentaAsync().Returns(((Guid?)null, (string?)null));

    private static CategoriaItem CrearCategoria(string nombre, TipoMovimiento tipo) => new()
    {
        IdCuentaCategoria = Guid.NewGuid(),
        Nombre = nombre,
        TipoMovimiento = tipo
    };

    // ── Estado inicial ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_InitialState_MatchesExpectedSnapshot()
    {
        var sut = CreateSut();

        Assert.Multiple(
            () => Assert.False(sut.IsBusy),
            () => Assert.Empty(sut.Categorias),
            () => Assert.Equal(string.Empty, sut.Error),
            () => Assert.Equal(string.Empty, sut.NombreCuenta),
            () => Assert.True(sut.SinCategorias),
            () => Assert.False(sut.HasCategorias),
            () => Assert.False(sut.HasError)
        );
    }

    // ── SinCategorias / HasCategorias ─────────────────────────────────────────

    [Fact]
    public async Task CargarCategoriasAsync_WhenCategoriasLoaded_UpdatesCategoriasFlags()
    {
        ConfigurarCuenta();
        _cuentasService.GetCategoriasAsync(IdCuenta)
            .Returns([CrearCategoria("Alimentación", TipoMovimiento.Gasto)]);

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.Multiple(
            () => Assert.False(sut.SinCategorias),
            () => Assert.True(sut.HasCategorias)
        );
    }

    [Fact]
    public async Task CargarCategoriasAsync_WhenHasError_CategoriasFlagsAreFalse()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.Multiple(
            () => Assert.False(sut.SinCategorias),
            () => Assert.False(sut.HasCategorias)
        );
    }

    // ── HasError ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task HasError_WhenServiceThrows_IsTrue()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.True(sut.HasError);
    }

    [Fact]
    public async Task HasError_WhenLoadedSuccessfully_IsFalse()
    {
        ConfigurarCuenta();
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns([]);

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.False(sut.HasError);
    }

    // ── NombreCuenta ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CargarCategoriasAsync_WhenCuentaLoaded_SetsNombreCuenta()
    {
        ConfigurarCuenta("Cuenta personal");
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns([]);

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.Equal("Cuenta personal", sut.NombreCuenta);
    }

    // ── Sin cuenta ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CargarCategoriasAsync_WhenNoCuenta_CategoriasSinCategorias()
    {
        ConfigurarSinCuenta();

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.Multiple(
            () => Assert.Empty(sut.Categorias),
            () => Assert.True(sut.SinCategorias),
            () => Assert.False(sut.HasError)
        );
    }

    // ── Ordenación ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CargarCategoriasAsync_WhenMultipleCategorias_SortsbyTipoThenNombre()
    {
        ConfigurarCuenta();
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns(
        [
            CrearCategoria("Sueldo", TipoMovimiento.Ingreso),
            CrearCategoria("Alimentación", TipoMovimiento.Gasto),
            CrearCategoria("Transporte", TipoMovimiento.Gasto),
            CrearCategoria("Inversión", TipoMovimiento.Ingreso),
        ]);

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        var nombres = sut.Categorias.Select(c => c.Nombre).ToList();
        Assert.Equal(["Inversión", "Sueldo", "Alimentación", "Transporte"], nombres);
    }

    // ── IsBusy ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CargarCategoriasAsync_AfterCompletion_IsBusyIsFalse()
    {
        ConfigurarCuenta();
        _cuentasService.GetCategoriasAsync(IdCuenta).Returns([]);

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task CargarCategoriasAsync_WhenServiceThrows_IsBusyIsFalse()
    {
        _cuentasService.GetDefaultCuentaAsync().Throws(new HttpRequestException("fallo"));

        var sut = CreateSut();
        await sut.CargarCategoriasAsync();

        Assert.False(sut.IsBusy);
    }
}
