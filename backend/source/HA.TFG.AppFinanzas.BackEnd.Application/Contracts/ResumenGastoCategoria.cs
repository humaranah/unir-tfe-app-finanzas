namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

/// <summary>
/// Proyección agregada del total de gasto de una categoría en un mes concreto.
/// </summary>
/// <param name="Anio">Año del periodo agregado.</param>
/// <param name="Mes">Mes del periodo agregado (1-12).</param>
/// <param name="IdCuentaCategoria">Identificador de la categoría de la cuenta.</param>
/// <param name="Categoria">Nombre de la categoría.</param>
/// <param name="Moneda">Moneda de los importes agregados.</param>
/// <param name="Total">Total gastado en la categoría durante el periodo.</param>
public sealed record ResumenGastoCategoria(
    int Anio,
    int Mes,
    Guid IdCuentaCategoria,
    string Categoria,
    string Moneda,
    decimal Total);
