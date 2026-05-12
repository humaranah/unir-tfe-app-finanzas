namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientosQuery;

public record GetMovimientosResultItem
{
    public Guid Id { get; init; }
    public Guid IdCuenta { get; init; }
    public Guid? IdCategoria { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
    public DateOnly Fecha { get; init; }
    public decimal Importe { get; init; }
    public string Concepto { get; init; } = string.Empty;
}
