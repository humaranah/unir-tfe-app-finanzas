using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Transacciones.GetTransaccionesQuery;

public record GetTransaccionesQuery : IRequest<IReadOnlyList<GetTransaccionesResultItem>>
{
    public required long IdCuenta { get; init; }
    public required string EmailUsuario { get; init; }
    public long? IdCategoria { get; init; }
    public DateOnly? FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }
}
