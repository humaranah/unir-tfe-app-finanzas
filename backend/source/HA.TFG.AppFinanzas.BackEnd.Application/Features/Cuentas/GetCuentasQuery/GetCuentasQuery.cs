using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;

public record GetCuentasQuery(
    string Email) : IRequest<IReadOnlyList<GetCuentasResultItem>>;
