using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;

public record GetCuentaCategoriasQuery(
    string Email,
    Guid IdCuenta) : IRequest<IReadOnlyList<GetCuentaCategoriasResultItem>>;
