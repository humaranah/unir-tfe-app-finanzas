namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Transacciones.GetTransaccionesQuery;

public record GetTransaccionesResultItem
{
    public long Id { get; init; }
    public long IdCuenta { get; init; }
    public long? IdCategoria { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
    public DateOnly Fecha { get; init; }
    public decimal Importe { get; init; }
    public string Concepto { get; init; } = string.Empty;
}
