using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Features.Cuentas;

namespace HA.TFG.AppFinanzas.Core.Tests.Fixtures;

public class CategoriaItemBuilder
{
    private Guid _idCuentaCategoria = Guid.NewGuid();
    private string _nombre = "Test Category";
    private TipoMovimiento _tipo = TipoMovimiento.Gasto;

    public CategoriaItemBuilder WithIdCuentaCategoria(Guid id)
    {
        _idCuentaCategoria = id;
        return this;
    }

    public CategoriaItemBuilder WithName(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    public CategoriaItemBuilder WithType(TipoMovimiento tipo)
    {
        _tipo = tipo;
        return this;
    }

    public CategoriaItem Build() => new CategoriaItem
    {
        IdCuentaCategoria = _idCuentaCategoria,
        Nombre = _nombre,
        TipoMovimiento = _tipo
    };
}
