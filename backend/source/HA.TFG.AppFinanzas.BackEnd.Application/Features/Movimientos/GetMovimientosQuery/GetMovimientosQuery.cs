using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientosQuery;

public record GetMovimientosQuery : IRequest<IReadOnlyList<GetMovimientosResultItem>>
{
    public required Guid IdCuenta { get; init; }
    public required string EmailUsuario { get; init; }
    public Guid? IdCategoria { get; init; }
    public DateOnly? FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }
}
