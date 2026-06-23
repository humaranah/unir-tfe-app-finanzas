using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;

namespace HA.TFG.AppFinanzas.Core.Tests.Fixtures;

public class MovimientoItemBuilder
{
    private Guid _idMovimiento = Guid.NewGuid();
    private Guid _idCuenta = Guid.NewGuid();
    private Guid? _idCategoria = null;
    private string? _nombreCategoria = null;
    private TipoMovimiento _tipo = TipoMovimiento.Gasto;
    private string _concepto = "Test Concept";
    private decimal _importe = 100m;
    private string _moneda = "USD";
    private DateOnly _fecha = DateOnly.FromDateTime(DateTime.Now);

    public MovimientoItemBuilder WithIdCuenta(Guid idCuenta)
    {
        _idCuenta = idCuenta;
        return this;
    }

    public MovimientoItemBuilder WithMoneda(string moneda)
    {
        _moneda = moneda;
        return this;
    }

    public MovimientoItemBuilder WithConcept(string concepto)
    {
        _concepto = concepto;
        return this;
    }

    public MovimientoItemBuilder WithAmount(decimal importe)
    {
        _importe = importe;
        return this;
    }

    public MovimientoItemBuilder WithType(TipoMovimiento tipo)
    {
        _tipo = tipo;
        return this;
    }

    public MovimientoItemBuilder WithCategory(Guid categoryId, string categoryName)
    {
        _idCategoria = categoryId;
        _nombreCategoria = categoryName;
        return this;
    }

    public MovimientoItemBuilder WithDate(DateOnly fecha)
    {
        _fecha = fecha;
        return this;
    }

    public MovimientoItem Build() => new MovimientoItem
    {
        IdMovimiento = _idMovimiento,
        IdCuenta = _idCuenta,
        IdCategoria = _idCategoria,
        NombreCategoria = _nombreCategoria,
        TipoMovimiento = _tipo,
        Concepto = _concepto,
        Importe = _importe,
        Moneda = _moneda,
        FechaMovimiento = _fecha
    };
}
