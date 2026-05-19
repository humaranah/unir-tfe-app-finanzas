using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetComprobanteMovimientoQuery;

public record GetComprobanteMovimientoQuery(
    string Email,
    Guid IdCuenta,
    Guid IdMovimiento) : IRequest<ComprobanteFile>;
