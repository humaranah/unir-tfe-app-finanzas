using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Persistence.Repositories;

public class MovimientoRepositoryTests : AppDbContextTestBase
{
    private readonly MovimientoRepository _sut;

    private static readonly Guid IdCuenta = Guid.CreateVersion7();
    private static readonly Guid IdCategoria = Guid.CreateVersion7();
    private static readonly DateTime Hoy = new(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);

    public MovimientoRepositoryTests()
    {
        _sut = new MovimientoRepository(Context);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CuentaCategoria BuildCategoria(Guid? id = null, Guid? idCuenta = null, string nombre = "Alimentación") =>
        new()
        {
            IdCuentaCategoria = id ?? IdCategoria,
            IdCuenta = idCuenta ?? IdCuenta,
            Nombre = nombre,
            TipoMovimiento = TipoMovimiento.Gasto,
            FechaCreacion = DateTime.UtcNow
        };

    private static Movimiento BuildMovimiento(
        Guid idCuentaCategoria,
        DateTime fecha,
        decimal importe,
        TipoMovimiento tipo = TipoMovimiento.Gasto,
        string moneda = "EUR",
        Guid? idCuenta = null) =>
        new()
        {
            IdMovimiento = Guid.CreateVersion7(),
            IdCuenta = idCuenta ?? IdCuenta,
            IdCuentaCategoria = idCuentaCategoria,
            TipoMovimiento = tipo,
            Concepto = "Test",
            Importe = importe,
            Moneda = moneda,
            FechaMovimiento = fecha,
            FechaCreacion = DateTime.UtcNow,
            Nota = string.Empty
        };

    private async Task SeedAsync(IEnumerable<CuentaCategoria> categorias, IEnumerable<Movimiento> movimientos)
    {
        Context.CuentaCategorias.AddRange(categorias);
        Context.Movimientos.AddRange(movimientos);
        await Context.SaveChangesAsync();
    }

    // ─── GetResumenGastosPorCategoriaAsync ───────────────────────────────────

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_SinMovimientos_DevuelveListaVacia()
    {
        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy.AddMonths(-1)),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_SoloIngresos_DevuelveListaVacia()
    {
        var categoria = BuildCategoria();
        var ingreso = BuildMovimiento(IdCategoria, Hoy, 100m, TipoMovimiento.Ingreso);
        await SeedAsync([categoria], [ingreso]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy.AddDays(-1)),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_GastoFueraDeRango_DevuelveListaVacia()
    {
        var categoria = BuildCategoria();
        var gasto = BuildMovimiento(IdCategoria, Hoy.AddMonths(-2), 50m);
        await SeedAsync([categoria], [gasto]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy.AddMonths(-1)),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_UnGasto_DevuelveResumenCorrecto()
    {
        var categoria = BuildCategoria();
        var gasto = BuildMovimiento(IdCategoria, Hoy, 75.50m);
        await SeedAsync([categoria], [gasto]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        var item = Assert.Single(result);
        Assert.Equal(Hoy.Year, item.Año);
        Assert.Equal(Hoy.Month, item.Mes);
        Assert.Equal(IdCategoria, item.IdCuentaCategoria);
        Assert.Equal("Alimentación", item.NombreCategoria);
        Assert.Equal("EUR", item.Moneda);
        Assert.Equal(75.50m, item.Total);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_VariosGastosMismaCategoria_AcumulaTotal()
    {
        var categoria = BuildCategoria();
        await SeedAsync([categoria], [
            BuildMovimiento(IdCategoria, Hoy, 30m),
            BuildMovimiento(IdCategoria, Hoy, 20m),
            BuildMovimiento(IdCategoria, Hoy, 50m)
        ]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        var item = Assert.Single(result);
        Assert.Equal(100m, item.Total);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_DosCategorias_DevuelveDosResumenes()
    {
        var idCat2 = Guid.CreateVersion7();
        var categoria1 = BuildCategoria(nombre: "Alimentación");
        var categoria2 = BuildCategoria(id: idCat2, nombre: "Transporte");
        await SeedAsync([categoria1, categoria2], [
            BuildMovimiento(IdCategoria, Hoy, 40m),
            BuildMovimiento(idCat2,      Hoy, 25m)
        ]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.NombreCategoria == "Alimentación" && r.Total == 40m);
        Assert.Contains(result, r => r.NombreCategoria == "Transporte" && r.Total == 25m);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_GastosEnMesesDistintos_DevuelveUnaFilaPorMes()
    {
        var categoria = BuildCategoria();
        await SeedAsync([categoria], [
            BuildMovimiento(IdCategoria, Hoy,                  60m),
            BuildMovimiento(IdCategoria, Hoy.AddMonths(-1),    40m)
        ]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy.AddMonths(-1)),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_ResultadoOrdenadoPorFechaDescendente()
    {
        var categoria = BuildCategoria();
        await SeedAsync([categoria], [
            BuildMovimiento(IdCategoria, Hoy.AddMonths(-2), 10m),
            BuildMovimiento(IdCategoria, Hoy,               10m),
            BuildMovimiento(IdCategoria, Hoy.AddMonths(-1), 10m)
        ]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy.AddMonths(-2)),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].Mes >= result[1].Mes);
        Assert.True(result[1].Mes >= result[2].Mes);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_SoloDevuelveGastosDelaCuentaSolicitada()
    {
        var otraCuenta = Guid.CreateVersion7();
        var idCatOtra = Guid.CreateVersion7();
        var catPropia = BuildCategoria();
        var catAjena = BuildCategoria(id: idCatOtra, idCuenta: otraCuenta);
        await SeedAsync([catPropia, catAjena], [
            BuildMovimiento(IdCategoria, Hoy, 100m, idCuenta: IdCuenta),
            BuildMovimiento(idCatOtra,   Hoy, 999m, idCuenta: otraCuenta)
        ]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            DateOnly.FromDateTime(Hoy),
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        var item = Assert.Single(result);
        Assert.Equal(100m, item.Total);
    }

    [Fact]
    public async Task GetResumenGastosPorCategoriaAsync_GastoEnFechaBorde_LoIncluyeEnElResultado()
    {
        var categoria = BuildCategoria();
        var fechaDesde = new DateOnly(Hoy.Year, Hoy.Month, 1);
        var gasto = BuildMovimiento(IdCategoria, fechaDesde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), 55m);
        await SeedAsync([categoria], [gasto]);

        var result = await _sut.GetResumenGastosPorCategoriaAsync(
            IdCuenta,
            fechaDesde,
            DateOnly.FromDateTime(Hoy),
            TestContext.Current.CancellationToken);

        Assert.Single(result);
    }
}
