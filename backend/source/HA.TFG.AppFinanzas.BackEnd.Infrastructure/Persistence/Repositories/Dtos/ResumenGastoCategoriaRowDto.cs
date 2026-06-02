namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories.Dtos;

internal sealed record ResumenGastoCategoriaRowDto(
    int Año,
    int Mes,
    Guid IdCuentaCategoria,
    string NombreCategoria,
    string Moneda,
    decimal Total);
