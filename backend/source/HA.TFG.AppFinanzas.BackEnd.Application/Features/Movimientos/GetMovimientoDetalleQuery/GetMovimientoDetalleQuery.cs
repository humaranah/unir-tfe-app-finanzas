using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientoDetalleQuery;

public record GetMovimientoDetalleQuery(
    string Email,
    Guid IdCuenta,
    Guid IdMovimiento) : IRequest<GetMovimientoDetalleResult>;
